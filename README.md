Generic Msmq Message Processing
============================
By Alwyn Lombaard

Usage
-----

Declare a message to process.

```C#
[Serializable]
public struct MyMessage : IMessage
{

}
``` 

###Server###

Declare a handler for it.

```C#
public class MyMessageHandler : IMessageHandler <MyMessage>
{
	public void HandleMessage(MyMessage message)
	{
		//handle your message here
		...
	}
	
	public void OnError(MyMessage message, Exception ex)
	{
		//handle your errors here
		...
	}

	public void Dispose()
	{
		//this is invoked after the message has been handled
		...
	}
}
``` 

####Start message processor(s).####

By using a factory:

```C#
//if you have dependencies in your Handlers to be resolved,
//pass in your container's Func<Type, object> serviceLocator 
//for example Kernel.GetService or Container.Resolve
var messageProcessorCollection = MessageProcessorCollectionFactory.Collection(Kernel.GetService);

//OR
//if your handlers have parameterless ctors
var messageProcessorCollection = MessageProcessorCollectionFactory.Collection();
								
messageProcessorCollection.StartAll();							

```

To stop processors:
```C#
messageProcessorCollection.StopAll();
```

**OR** 

do it explicitly:

```C#
var inboundMessageQueue = new MsmqMessageQueueInbound<MyMessage>();

var messageHandler = new MyMessageHandler()

var messageProcessor = new MessageProcessor<MyMessage>( inboundMessageQueue, 
														messageHandler);

messageProcessor.Start();
``` 

To stop the processor:

```C#
messageProcessor.Stop();
```


###Client###

Add a message to the queue.

```C#
...
try
{
	var queue = new MsmqMessageQueueOutbound<MyMessage>();
	var message = new MyMessage();
	queue.Send(message);
}
catch (Exception ex)...
``` 

Msmq implementation based on https://github.com/michaellperry