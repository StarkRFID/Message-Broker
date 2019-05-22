//  ------------------------------------------------------------------------------
// Copyright © Stark EM, LLC 2019 - All Rights Reserved
// Unauthorized copying of this file, via any medium, is strictly prohibited.
// ------------------------------------------------------------------------------

using NUnit.Framework;
using Stark.MessageBroker.Tests.Mocks;

namespace Stark.MessageBroker.Tests
{
    public abstract class TestBase
    {
        /// <summary>
        /// Configures the dependency injection framework (Autofac) for use.
        /// </summary>
        /// <returns></returns>
        [OneTimeSetUp]
        public void FixtureSetup()
        {
            //Bootstrapper.Initialize(bs => {
            //                            bs.RegisterType<LogServiceMock>().AsImplementedInterfaces();
            //                        });
        }
    }
}