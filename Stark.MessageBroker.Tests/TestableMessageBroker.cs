// ------------------------------------------------------------------------------
// Copyright © Stark EM, LLC 2019 - All Rights Reserved
// Unauthorized copying of this file, via any medium, is strictly prohibited.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Stark.MessageBroker.Tests
{
    /// <summary>
    /// This wrapper should only be used in unit tests to provide access
    /// to protected / private methods and variables.
    /// </summary>
    internal sealed class TestableMessageBroker : MessageBroker
    {
        /// <summary>
        /// Returns the distinct Action names.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetActionNames()
        {
            return Actions.Values?.SelectMany(p => p).Select(p => p.Name).Distinct().ToList();
        }

        /// <summary>
        /// Returns the distinct message types.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetActionMessageTypes()
        {
            return Actions.Keys.ToList();
        }

        /// <summary>
        /// Provides access to the underlying method for test purposes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageType"></param>
        /// <param name="action"></param>
        public new void Subscribe<T>(string messageType, (object Func, string Name) action) where T : IMessage
        {
            base.Subscribe<T>(messageType, action);
        }
    }
}