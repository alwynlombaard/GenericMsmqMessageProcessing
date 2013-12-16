Generic Msmq Message Processing
============================
By Alwyn Lombaard

Usage
-----

Declare a message to process.

```C#
class MyMessage : IMessage
{

}
``` 

###Server###

Declare a handler for it.

```C#
class MyMessageHandler : IMessageHandler <MyMessage>
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
		
	}
}
``` 
At app start

Start message processor(s) by using a factory.


```C#
//if you have dependencies in your Handlers to be resolved,
//pass in your container's Func<Type, object> serviceLocator 
//for example Kernel.GetService or Container.Resolve
var messageProcessorCollection = MessageProcessorCollectionFactory.Collection(Kernel.GetService);

//OR
//if your handlers have parameterless ctors
var messageProcessorCollection = MessageProcessorCollectionFactory.Collection(logger);
								
messageProcessorCollection.StartAll();							

```

To stop processors:
```C#
messageProcessorCollection.StopAll();
```

Or do it explicitly:

```C#
var inboundMessageQueue = new MsmqMessageQueueInbound<MyMessage>(logger);

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

###Container setup###

Ninject example bindings for MsmqMessageQueueInbound dependency.

```C#
Bind<ILog>()
.ToMethod(x =>
{
	var type = x.Request.ParentRequest != null 
		? x.Request.ParentRequest.Service 
		: x.Request.Service;
	return LogManager.GetLogger(type);
});
```

