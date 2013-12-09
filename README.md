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

	public void Dispose()
	{
		
	}
}
``` 


Start a message processor. Typically at app start. 

```C#
var inboundMessageQueue = new MsmqMessageQueueInbound<MyMessage>(logger);

var messageHandler = new MyMessageHandler()

var messageProcessor = new MessageProcessor<MyMessage>(	logger, 
														inboundMessageQueue, 
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

Ninject example bindings MessageProcessor

```C#
Bind(typeof (IMessageQueueInbound<>))
.To(typeof (MsmqMessageQueueInbound<>));

Bind(typeof(IMessageHandler<>))
.To(typeof(MessageHandler<>));

Bind<ILog>()
.ToMethod(x =>
{
	var type = x.Request.ParentRequest != null 
		? x.Request.ParentRequest.Service 
		: x.Request.Service;
	return LogManager.GetLogger(type);
});
```


Based on Msmq code by https://github.com/michaellperry
