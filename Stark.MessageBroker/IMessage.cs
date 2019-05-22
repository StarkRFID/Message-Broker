// ------------------------------------------------------------------------------
// Copyright © Stark EM, LLC 2019 - All Rights Reserved
// Unauthorized copying of this file, via any medium, is strictly prohibited.
// ------------------------------------------------------------------------------

namespace Stark.MessageBroker
{
    /// <summary>
    /// Messages handled by the Message Broker.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// The type of message to that will be posted.
        /// </summary>
        string Type { get; }
    }

    /// <summary>
    /// Messages that include data handled by the Message Broker.
    /// </summary>
    public interface IMessageWithData : IMessage
    {
        object Data { get; }
    }

    /// <summary>
    /// Messages that include data handled by the Message Broker.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public interface IMessage<TData> : IMessageWithData
    {
        /// <summary>
        /// The data passed to the Action.
        /// </summary>
        new TData Data { get; set; }
    }
}