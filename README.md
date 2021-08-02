# Message-Broker

A simple, asynchronous, in-memory message broker that provides publish/subscribe functionality between loosely coupled components.

The library makes use of LibLog to provide logging abstraction. For more information, see: <https://github.com/damianh/LibLog.>

## NuGet

Package can be download from: [Stark Message-Broker](https://www.nuget.org/packages/Stark.MessageBroker/).

## Usage

Messages are posted and subscribed to by type, with the ability to execute custom actions for each message type.

In the most basic example, a generic message with no data is posted to the broker and forwarded to subscribers, with
each function passed in the subscription executed.

```c#
// Message class
public class GenericMessage : Message
{
}

// Subscribe to messages and perform action when received
broker.Subscribe<GenericMessage>(() =>
                                 {
                                     // do interesting things...
                                     return Task.CompletedTask;
                                 },
                                 "subscription name");

// Post message
await broker.PostAsync<GenericMessage>();
```

Message Broker also supports posting messages with instance data, which is then available to the executed function.

```c#
// Message class
public class FooMessage : Message<Foo>
{
}

// Subscribe to messages and perform action when received
broker.Subscribe<FooMessage>(fooMsg =>
                            {
                                var data = fooMsg.Data.Data;
                                // do interesting things with data...
                            },
                            "subscription name");

// Post message
await broker.PostAsync(new FooMessage());
```
