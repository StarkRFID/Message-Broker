// ------------------------------------------------------------------------------
// Copyright © Stark EM, LLC 2019 - All Rights Reserved
// Unauthorized copying of this file, via any medium, is strictly prohibited.
// ------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace Stark.MessageBroker
{
    [Serializable]
    public class MessageBrokerException : Exception
    {
        public MessageBrokerException() { }
        public MessageBrokerException(string msg) : base(msg) { }
        public MessageBrokerException(string msg, Exception innerEx) : base(msg, innerEx) { }
        private MessageBrokerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}