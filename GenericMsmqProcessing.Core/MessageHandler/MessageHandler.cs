using ReallySimpleEventing;

namespace GenericMsmqProcessing.Core.MessageHandler
{
    public class MessageHandler<T> : IMessageHandler<T>
    {
        private readonly IEventStream _eventStream;

        public MessageHandler(IEventStream eventStream)
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