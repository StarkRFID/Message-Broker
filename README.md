# Message-Broker

A simple, asynchronous, in-memory message broker. The goal was to create a simple way for two components to communicate
with each other without knowing about each other.

The library makes use of LibLog to provide logging abstraction. For more information, see: <https://github.com/damianh/LibLog.>

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
                                     LogService.Log(LogLevel.Info,
                                                    "Action {name} Executed for test: {0}.",
                                                    name,
                                                    nameof(SubscribeNoDataTest));
                                     return Task.CompletedTask;
                                 },
                                 name);

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
                                executedActions.Add($"{nameof(FooMessage)} Executed with data `{fooMsg.Data.Data}`");
                                return Task.CompletedTask;
                            },
                            "Action 1");

// Post message
await broker.PostAsync(new FooMessage());
```
