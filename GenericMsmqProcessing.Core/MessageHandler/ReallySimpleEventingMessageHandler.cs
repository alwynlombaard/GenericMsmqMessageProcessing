using ReallySimpleEventing;

namespace GenericMsmqProcessing.Core.MessageHandler
{
    public class ReallySimpleEventingMessageHandler<T> : IMessageHandler<T>
    {
        private readonly IEventStream _eventStream;

        public ReallySimpleEventingMessageHandler(IEventStream eventStream)
        {
            _eventStream = eventStream;
        }

        public void HandleMessage(T message)
        {
           _eventStream.Raise(message);
        }
        
        public void Dispose()
        {
           
        }
    }
}