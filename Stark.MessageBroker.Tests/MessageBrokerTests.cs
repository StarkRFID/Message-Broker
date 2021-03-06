﻿// ------------------------------------------------------------------------------
// Copyright © Stark EM, LLC 2019 - All Rights Reserved
// Unauthorized copying of this file, via any medium, is strictly prohibited.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dasync.Collections;
using NUnit.Framework;
using Stark.Messaging.Tests.Messages;
using Stark.Messaging.Tests.Mocks;

namespace Stark.Messaging.Tests
{
    [TestFixture]
    [Category("Service")]
    public class MessageBrokerTests
    {
        private LogServiceMock LogService { get; set; }

        [OneTimeSetUp]
        public void Initialize()
        {
            LogService = new LogServiceMock();
        }

        /// <summary>
        /// Gets a new broker instance and verifies it's empty.
        /// </summary>
        /// <returns></returns>
        private TestableMessageBroker GetFreshBroker()
        {
            var broker = new TestableMessageBroker();
            Assert.That(broker, Is.Not.Null);
            Assert.That(broker.GetActionNames().Any(), Is.False);

            return broker;
        }

        [Test]
        public void CheckIfExistsTest()
        {
            const string action = "Action";
            const string actionWithData = "Action with Data";

            // Use the wrapper to verify initial configuration is empty.
            var broker = GetFreshBroker();

            broker.Subscribe<GenericMessage>(() =>
                                             {
                                                 LogService.Log(LogLevel.Info,
                                                                "Action Executed for test: {0}.",
                                                                nameof(CheckIfExistsTest));
                                                 return Task.CompletedTask;
                                             },
                                             action);
            broker.Subscribe<FooMessage>(rtr =>
                                                 {
                                                     LogService.Log(LogLevel.Info,
                                                                    "Action with Data Executed for test: {0}.",
                                                                    nameof(CheckIfExistsTest));
                                                     return Task.CompletedTask;
                                                 },
                                                 actionWithData);

            Assert.Throws<ArgumentNullException>(() => broker.CheckIfExists(" "));

            var exists = broker.CheckIfExists(action);
            Assert.That(exists, Is.True);
            exists = broker.CheckIfExists(actionWithData);
            Assert.That(exists, Is.True);
        }

        [Test]
        public void SubscribeNoDataTest()
        {
            const string name = "Action Jackson";

            // Need to use the wrapper so we can verify Subscribe works
            // without needing to use CheckIfExists().
            var broker = GetFreshBroker();

            broker.Subscribe<GenericMessage>(() =>
                                             {
                                                 LogService.Log(LogLevel.Info,
                                                                "Action {name} Executed for test: {0}.",
                                                                name,
                                                                nameof(SubscribeNoDataTest));
                                                 return Task.CompletedTask;
                                             },
                                             name);

            Assert.Throws<ArgumentNullException>(() => broker.Subscribe<GenericMessage>((Func<Task>)null));

            var namePresentInAction = broker.GetActionNames().Any(p => p.Equals(name));
            Assert.That(namePresentInAction, Is.True);
        }

        [Test]
        public void SubscribeWithDataTest()
        {
            const string name = "Action Jackson";

            // Need to use the wrapper so we can verify Subscribe works
            // without needing to use CheckIfExists().
            var broker = GetFreshBroker();

            broker.Subscribe<FooMessage>(rti =>
                                                 {
                                                     LogService.Log(LogLevel.Info,
                                                                    "Action {name} Executed for test: {0}.",
                                                                    name,
                                                                    nameof(SubscribeWithDataTest));
                                                     return Task.CompletedTask;
                                                 },
                                                 name);

            Assert.Throws<ArgumentNullException>(() => broker.Subscribe<FooMessage>((Func<FooMessage, Task>)null));


            var namePresentInAction = broker.GetActionNames().Any(p => p.Equals(name));
            Assert.That(namePresentInAction, Is.True);
        }

        /// <summary>
        /// Tests the protected Subscribe method. This test is just to verify that
        /// the "messageType" cannot be null. The other subscribe tests verify
        /// its core functionality.
        /// </summary>
        [Test]
        public void ProtectedSubscribeTest()
        {
            Task Func()
            {
                return Task.CompletedTask;
            }

            Assert.Throws<ArgumentNullException>(() => GetFreshBroker()
                                                     .Subscribe<GenericMessage>(" ", ((Func<Task>) Func, Name: "no-op")));
        }

        [Test]
        public void UnsubscribeByNameTest()
        {
            const string name = "Action";
            const string remains = "Remains";

            // Need to use the wrapper so we can verify Subscribe works
            // without needing to use CheckIfExists().
            var broker = GetFreshBroker();

            broker.Subscribe<GenericMessage>(() =>
                                             {
                                                 LogService.Log(LogLevel.Info,
                                                                "Action {name} Executed for test: {0}.",
                                                                name,
                                                                nameof(UnsubscribeByNameTest));
                                                 return Task.CompletedTask;
                                             },
                                             name);

            broker.Subscribe<GenericMessage>(() =>
                                             {
                                                 LogService.Log(LogLevel.Info,
                                                                "Action {remains} Executed for test: {0}.",
                                                                remains,
                                                                nameof(UnsubscribeByNameTest));
                                                 return Task.CompletedTask;
                                             },
                                             remains);

            broker.Subscribe<FooMessage>(rti =>
                                                 {
                                                     LogService.Log(LogLevel.Info,
                                                                    "Action {name} Executed for test: {0}.",
                                                                    name,
                                                                    nameof(UnsubscribeByNameTest));
                                                     return Task.CompletedTask;
                                                 },
                                                 name);

            broker.Subscribe<FooMessage>(rti =>
                                                 {
                                                     LogService.Log(LogLevel.Info,
                                                                    "Action {remains} Executed for test: {0}.",
                                                                    remains,
                                                                    nameof(UnsubscribeByNameTest));
                                                     return Task.CompletedTask;
                                                 },
                                                 remains);

            // Invoke the method under test
            broker.Unsubscribe(name);

            // Verify
            Assert.Throws<ArgumentNullException>(() => broker.Unsubscribe(" "));

            Assert.That(broker.GetActionNames().Any(p => p.Equals(name)), Is.False);
            Assert.That(broker.GetActionNames().Any(p => p.Equals(remains)), Is.True);
        }

        [Test]
        public void UnsubscribeByTypeTest()
        {
            // Need to use the wrapper so we can verify Subscribe works
            // without needing to use CheckIfExists().
            var broker = GetFreshBroker();

            broker.Subscribe<GenericMessage>(() =>
                                             {
                                                 LogService.Log(LogLevel.Info,
                                                                "Action {name} Executed for test: {0}.",
                                                                nameof(GenericMessage),
                                                                nameof(UnsubscribeByNameTest));
                                                 return Task.CompletedTask;
                                             });

            broker.Subscribe<FooMessage>(rti =>
                                                 {
                                                     LogService.Log(LogLevel.Info,
                                                                    "Action {name} Executed for test: {0}.",
                                                                    nameof(FooMessage),
                                                                    nameof(UnsubscribeByNameTest));
                                                     return Task.CompletedTask;
                                                 });

            // Invoke the method under test
            broker.Unsubscribe<GenericMessage>();

            var namePresent = broker.GetActionMessageTypes().Any(p => p.Equals(typeof(GenericMessage).FullName));
            Assert.That(namePresent, Is.False);
            Assert.That(broker.GetActionMessageTypes().Any(p => p.Equals(typeof(FooMessage).FullName)), Is.True);
        }

        [Test]
        public void UnsubscribeAllTest()
        {
            // Use the wrapper to verify initial configuration is empty.
            var broker = GetFreshBroker();

            broker.Subscribe<GenericMessage>(() =>
                                             {
                                                 LogService.Log(LogLevel.Info,
                                                                "Action {name} Executed for test: {0}.",
                                                                nameof(GenericMessage),
                                                                nameof(UnsubscribeByNameTest));
                                                 return Task.CompletedTask;
                                             });

            broker.Subscribe<FooMessage>(rti =>
                                                 {
                                                     LogService.Log(LogLevel.Info,
                                                                    "Action {name} Executed for test: {0}.",
                                                                    nameof(FooMessage),
                                                                    nameof(UnsubscribeByNameTest));
                                                     return Task.CompletedTask;
                                                 });

            // Invoke the method under test
            broker.UnsubscribeAll();

            // Verify
            Assert.That(broker.GetActionNames().Any(), Is.False);
            Assert.That(broker.GetActionMessageTypes().Any(), Is.False);
        }

        [Test]
        public async Task PostAsyncNoDataTest()
        {
            var executedActions = new List<string>();
            var broker = GetFreshBroker();

            broker.Subscribe<GenericMessage>(() =>
                                             {
                                                 executedActions.Add($"{nameof(GenericMessage)} Executed");
                                                 return Task.CompletedTask;
                                             },
                                             "Action 1");
            broker.Subscribe<DoNotRunMessage>(() =>
                                              {
                                                  executedActions.Add($"{nameof(DoNotRunMessage)} Executed");
                                                  return Task.CompletedTask;
                                              },
                                              "Action 2");

            // Invoke the method under test
            await broker.PostAsync<FooMessage>(); // should gracefully return
            await broker.PostAsync<GenericMessage>();

            // Verify
            Assert.That(executedActions.Count, Is.EqualTo(1));
            Assert.That(executedActions.Any(p => p.Contains($"{nameof(GenericMessage)}")), Is.True);
            Assert.That(executedActions.Any(p => p.Contains($"{nameof(DoNotRunMessage)}")), Is.False); // not really needed
        }

        [Test]
        public async Task PostAsyncWithDataTest()
        {
            var executedActions = new List<string>();
            var broker = GetFreshBroker();

            broker.Subscribe<FooMessage>(rti =>
                                         {
                                             executedActions.Add($"{nameof(FooMessage)} Executed");
                                             return Task.CompletedTask;
                                         },
                                         "Action 1");
            broker.Subscribe<BarMessage>(rti =>
                                         {
                                             executedActions.Add($"{nameof(BarMessage)} Executed");
                                             return Task.CompletedTask;
                                         },
                                         "Action 2");
            broker.Subscribe<FooMessage>(() =>
                                         {
                                             executedActions.Add($"{nameof(FooMessage)} Executed");
                                             return Task.CompletedTask;
                                         },
                                         "Action 3");


            // Invoke the method under test
            await broker.PostAsync(new DoNotRunWithData()); // should gracefully return
            await broker.PostAsync(new FooMessage());

            // Verify
            Assert.ThrowsAsync<ArgumentNullException>(() => broker.PostAsync<FooMessage>(null));

            Assert.That(executedActions.Count, Is.EqualTo(2));
            Assert.That(executedActions.Any(p => p.Contains($"{nameof(FooMessage)}")), Is.True);
            Assert.That(executedActions.Any(p => p.Contains($"{nameof(BarMessage)}")), Is.False);
        }

        [Test]
        public async Task PostAsyncStressTest([Values(10, 100, 1000, 10000)]int iteration)
        {
            var jitter = new Random();
            var result = new ConcurrentBag<string>();

            var broker = GetFreshBroker();
            broker.Subscribe<FooMessage>(async rti =>
                                                 {
                                                     await Task.Delay(jitter.Next(15, 221));
                                                     result.Add("Action with Data Executed");
                                                 },
                                                 "Action 1");
            broker.Subscribe<FooMessage>(async () =>
                                                 {
                                                     await Task.Delay(jitter.Next(45, 327));
                                                     result.Add("Action Executed");
                                                 },
                                                 "Action 2");

            var posts = new List<int>(iteration);
            for (var idx = 1; idx <= iteration; idx++) posts.Add(idx);

            await posts.ParallelForEachAsync(async post => await broker.PostAsync(new FooMessage()),
                                             maxDegreeOfParallelism: 100);

            Assert.That(result.Count, Is.EqualTo(2*iteration));
            Assert.That(result.Count(p => p.Contains("Action Executed")), Is.EqualTo(iteration));
            Assert.That(result.Count(p => p.Contains("Action with Data Executed")), Is.EqualTo(iteration));
        }

        [Test]
        public async Task PreventSubscriptionModificationsDuringExecution([Values]bool withData)
        {
            var executedActions = new List<string>();
            var broker = GetFreshBroker();

            var abort = false;

            var t0 = Task.Run(async () =>
                              {
                                  Console.WriteLine($"Starting: {DateTime.Now:O}");
                                  var timer = new Stopwatch();
                                  timer.Start();
                                  for (var i = 1; i <= 5000; i++) {
                                      //Console.WriteLine($"Iteration {i}: {DateTime.Now:O}");
                                      if (withData) {
                                          broker.Subscribe<FooMessage>(rti =>
                                                                       {
                                                                           executedActions.Add($"{nameof(FooMessage)} Executed");
                                                                           return Task.CompletedTask;
                                                                       },
                                                                       $"Action {i}");
                                      } else {
                                          broker.Subscribe<GenericMessage>(() =>
                                                                           {
                                                                               executedActions.Add($"{nameof(GenericMessage)} Executed");
                                                                               return Task.CompletedTask;
                                                                           },
                                                                           $"Action {i}");
                                      }

                                      if (abort) break;
                                      await Task.Delay(TimeSpan.FromMilliseconds(1));
                                  }
                                  timer.Stop();
                                  Console.WriteLine($"Duration: {timer.ElapsedMilliseconds}: {DateTime.Now:O}");
                              });

            var t1 = Task.Run(async () =>
                              {
                                  await Task.Delay(TimeSpan.FromMilliseconds(100));
                                  for (var i = 1; i <= 5000; i++) {
                                      try {
                                          if (withData) await broker.PostAsync(new FooMessage());
                                          else await broker.PostAsync<GenericMessage>();
                                      } catch (Exception) {
                                          Console.WriteLine($"Failed on iteration {i}");
                                          abort = true;
                                          throw;
                                      }

                                      if (abort) break;
                                      await Task.Delay(TimeSpan.FromMilliseconds(1));
                                  }
                              });
            var t2 = Task.Run(async () =>
                              {
                                  await Task.Delay(TimeSpan.FromMilliseconds(150));
                                  for (var i = 1; i <= 5000; i++) {
                                      try {
                                          if (withData) broker.Unsubscribe<FooMessage>();
                                          else broker.Unsubscribe<GenericMessage>();
                                      } catch (Exception) {
                                          Console.WriteLine($"Failed on iteration {i}");
                                          abort = true;
                                          throw;
                                      }

                                      if (abort) break;
                                      await Task.Delay(TimeSpan.FromMilliseconds(1));
                                  }
                              });

            await Task.WhenAll(new[] { t0, t1, t2 });
        }
    }
}