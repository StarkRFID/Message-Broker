// ------------------------------------------------------------------------------
// Copyright © Stark EM, LLC 2019 - All Rights Reserved
// Unauthorized copying of this file, via any medium, is strictly prohibited.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dasync.Collections;
using Stark.MessageBroker.Logging;


namespace Stark.MessageBroker
{
    public class MessageBroker : IMessageBroker
    {
        /// <summary>
        /// Generic logging provider
        /// </summary>
        private static readonly ILog LogService = LogProvider.For<MessageBroker>();

        protected ConcurrentDictionary<string, IList<(object Func, string Name)>> Actions { get; } =
            new ConcurrentDictionary<string, IList<(object Func, string Name)>>();

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public MessageBroker()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Checks to see if an Action with the given name exists in either of the collections.
        /// The check is case-sensitive.
        /// </summary>
        /// <param name="name"></param>
        public bool CheckIfExists(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            return Actions.Values.SelectMany(p => p).Any(p => p.Name == name);
        }

        /// <inheritdoc />
        /// <summary>
        /// Removes all actions registered with the broker.
        /// </summary>
        public void UnsubscribeAll()
        {
            Actions.Clear();
        }

        /// <inheritdoc />
        /// <summary>
        /// Removes all actions for the specified message type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Unsubscribe<T>()
        {
            // Find keys that match message type and attempt to remove. Throw on failure.
            var messageType = ResolveMessageType<T>();
            if (Actions.Keys.Where(key => key == messageType).Any(key => !Actions.TryRemove(messageType, out _))) {
                throw new MessageBrokerException($"Failed to unsubscribe all Actions for {messageType}!");
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Removes all actions with the given name. If multiple actions exist with the same
        /// name, all will be removed.
        /// </summary>
        /// <param name="name"></param>
        public void Unsubscribe(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            // bmg: This is weird, but needed when trying to update the value
            //      for a concurrent dictionary. 
            foreach (var messageType in Actions.Keys) {
                // todo: Throw if it fails?
                if (!Actions.TryGetValue(messageType, out var actions)) continue;
                lock (actions) {
                    Actions.TryUpdate(messageType, actions.Where(p => p.Name != name).ToList(), actions);
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Adds a new Action to be executed when a message of the given type is received.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="name"></param>
        public void Subscribe<T>(Func<Task> func, string name = "") where T : IMessage
        {
            if (null == func) throw new ArgumentNullException(nameof(func));
            Subscribe<T>(ResolveMessageType<T>(), (Func: func, Name: name));
        }

        /// <inheritdoc />
        /// <summary>
        /// Adds a new Action to be executed when a message of the given type is received.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="name"></param>
        public void Subscribe<T>(Func<T, Task> func, string name = "") where T : IMessageWithData
        {
            if (null == func) throw new ArgumentNullException(nameof(func));
            Subscribe<T>(ResolveMessageType<T>(), (Func: func, Name: name));
        }

        /// <summary>
        /// Adds a new Action to be executed when a message of the given type is received.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageType"></param>
        /// <param name="action"></param>
        /// <remarks>
        /// This method is protected to hide some of the gnarly implementation details on how to
        /// subscribe. The available overloads should make using the broker much easier
        /// and clearer.
        ///
        /// Many thanks to Alex for the inspiration and spirited discussion.
        /// </remarks>
        // ReSharper disable once UnusedTypeParameter
        protected void Subscribe<T>(string messageType, (object Func, string Name) action) where T : IMessage
        {
            if (string.IsNullOrWhiteSpace(messageType)) throw new ArgumentNullException(nameof(messageType));

            if (Actions.TryGetValue(messageType, out var actionValues)) {
                lock (actionValues) {
                    actionValues.Add(action);
                }
            } else {
                if (!Actions.TryAdd(messageType, new List<(object, string)> {action})) {
                    throw new MessageBrokerException($"Failed to add Action with Data (Name: {action.Name}) for {messageType} subscriptions.");
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Receives a new message to be processed and executes subscribed Actions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task PostAsync<T>() where T : IMessage
        {
            var messageType = ResolveMessageType<T>();
            LogService.InfoFormat("{Class}|{MessageType} message received.", nameof(MessageBroker), messageType);

            var actionsExist = Actions.ContainsKey(messageType);
            if (!actionsExist) {
                LogService.InfoFormat("{Class}|No Actions configured for {MessageType} messages.",
                                      nameof(MessageBroker),
                                      messageType);
                return;
            }

            // Execute Actions for the message type. The else{...} block exists as 
            // a simple fallback in the event that the original request for the 
            // Actions fails.
            if (Actions.TryGetValue(messageType, out var actionValues)) {
                // todo: Would like to lock to protect against changes to the collection...
                await actionValues.ParallelForEachAsync(async action =>
                                                        {
                                                            if (action.Func is Func<Task> f) await f();
                                                        },
                                                        maxDegreeOfParallelism: 100);
            } else {
                if (Actions.TryGetValue(messageType, out actionValues)) {
                    // todo: Would like to lock to protect against changes to the collection...
                    await actionValues.ParallelForEachAsync(async action =>
                                                            {
                                                                if (action.Func is Func<Task> f) await f();

                                                            },
                                                            maxDegreeOfParallelism: 100);
                } else {
                    throw new MessageBrokerException($"Failed to get Actions for {messageType}! Actions not processed.");
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Receives a new message to be processed and executes subscribed Actions.
        /// </summary>
        /// <param name="message"></param>
        public async Task PostAsync<T>(T message) where T : IMessageWithData
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            
            var messageType = ResolveMessageType<T>();
            LogService.InfoFormat("{Class}|{MessageType} message received. Data: {data}",
                                  nameof(MessageBroker),
                                  messageType,
                                  message.Data);

            var actionsExist = Actions.ContainsKey(messageType);
            if (!actionsExist) {
                LogService.InfoFormat("{Class}|No Actions configured for {MessageType} messages.",
                                      nameof(MessageBroker),
                                      messageType);
                return;
            }

            // Execute Actions for the message type. The else{...} block exists as 
            // a simple fallback in the event that the original request for the 
            // Actions fails.
            if (Actions.TryGetValue(messageType, out var actionValues)) {
                // todo: Would like to lock to protect against changes to the collection...
                await actionValues.ParallelForEachAsync(async action =>
                                                        {
                                                            if (action.Func is Func<Task> f) await f();
                                                            if (action.Func is Func<T, Task> fd) await fd(message);
                                                        },
                                                        maxDegreeOfParallelism: 100);
            } else {
                if (Actions.TryGetValue(messageType, out actionValues)) {
                    // todo: Would like to lock to protect against changes to the collection...
                    await actionValues.ParallelForEachAsync(async action =>
                                                            {
                                                                if (action.Func is Func<Task> f) await f();
                                                                if (action.Func is Func<T, Task> fd) await fd(message);
                                                            },
                                                            maxDegreeOfParallelism: 100);
                } else {
                    throw new MessageBrokerException($"Failed to get Actions for {messageType}! Actions not processed.");
                }
            }
        }

        /// <summary>
        /// Returns the message type based on the message class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static string ResolveMessageType<T>()
        {
            return typeof(T).FullName;
        }
    }
}