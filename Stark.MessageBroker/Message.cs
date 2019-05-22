// ------------------------------------------------------------------------------
// Copyright © Stark EM, LLC 2019 - All Rights Reserved
// Unauthorized copying of this file, via any medium, is strictly prohibited.
// ------------------------------------------------------------------------------

namespace Stark.MessageBroker
{
    /// <inheritdoc />
    /// <summary>
    /// The base class for all messages handled by the Message Broker.
    /// </summary>
    public abstract class Message : IMessage
    {
        /// <inheritdoc />
        /// <summary>
        /// The type of message to that will be posted.
        /// </summary>
        public virtual string Type => GetType().FullName;
    }

    /// <inheritdoc />
    /// <summary>
    /// The base class for all messages that include data handled by the Message Broker.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public abstract class Message<TData> : Message, IMessage<TData>
    {
        // bmg: This is required to allow proper constraints on the 
        //      subscribe and post methods. Looks a bit odd, but works.
        /// <summary>
        /// Provides access to the data passed to the Action.
        /// </summary>
        object IMessageWithData.Data => Data;

        /// <inheritdoc />
        /// <summary>
        /// The data passed to the Action.
        /// </summary>
        public TData Data { get; set; }
    }
}