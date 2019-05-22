//  ------------------------------------------------------------------------------
// Copyright © Stark EM, LLC 2019 - All Rights Reserved
// Unauthorized copying of this file, via any medium, is strictly prohibited.
// ------------------------------------------------------------------------------

using System;

namespace Stark.MessageBroker.Tests.Mocks
{
    internal class LogServiceMock
    {
        public void Log(LogLevel level, string messageTemplate, params object[] propertyValues)
        {
            var now = DateTimeOffset.Now;

            switch (level)
            {
                case LogLevel.Fatal:
                case LogLevel.Error:
                case LogLevel.Warning:
                    Console.WriteLine($"{now:s} [{level:G}]: {messageTemplate}");
                    break;
            }
        }

        public void LogException(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine($"{DateTimeOffset.Now:s} [Error]: {exception.Message} {messageTemplate}");
        }

        public void LogFatalException(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine($"{DateTimeOffset.Now:s} [Fatal]: {exception.Message} {messageTemplate}");
        }
    }
}