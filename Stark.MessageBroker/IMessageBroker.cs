// ------------------------------------------------------------------------------
// Copyright © Stark EM, LLC 2019 - All Rights Reserved
// Unauthorized copying of this file, via any medium, is strictly prohibited.
// ------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace Stark.MessageBroker
{
    public interface IMessageBroker
    {
        /// <summary>
        /// Checks to see if an Action with the given name exists in either of the collections.
        /// The check is case-sensitive.
        /// </summary>
        /// <param name="name"></param>
        bool CheckIfExists(string name);

        /// <summary>
        /// Removes all actions registered with the broker.
        /// </summary>
        void UnsubscribeAll();

        /// <summary>
        /// Removes all actions for the specified message type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void Unsubscribe<T>();

        /// <summary>
        /// Removes all actions with the given name. If multiple actions exist with the same
        /// name, all will be removed.
        /// </summary>
        /// <param name="name"></param>
        void Unsubscribe(string name);

        /// <summary>
        /// Adds a new Action to be executed when a message of the given type is received.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="name"></param>
        void Subscribe<T>(Func<Task> func, string name = "") where T : IMessage;

        /// <summary>
        /// Adds a new Action to be executed when a message of the given type is received.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="name"></param>
        void Subscribe<T>(Func<T, Task> func, string name = "") where T : IMessageWithData;

        /// <summary>
        /// Receives a new message to be processed and executes subscribed Actions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task PostAsync<T>() where T : IMessage;

        /// <summary>
        /// Receives a new message to be processed and executes subscribed Actions.
        /// </summary>
        /// <param name="message"></param>
        Task PostAsync<T>(T message) where T : IMessageWithData;
    }
}