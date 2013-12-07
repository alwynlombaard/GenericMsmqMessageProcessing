using log4net;
using ReallySimpleEventing;

namespace GenericMsmqProcessing.Core.MessageHandler
{
    public class MessageHandler<T> : IMessageHandler<T>
    {
        private readonly IEventStream _eventStream;
        private readonly ILog _log;

        public MessageHandler(IEventStream eventStream, ILog log)
        {
            _eventStream = eventStream;
            _log = log;
        }

        public void HandleMessage(T message)
        {
           _eventStream.Raise(message);
           _log.InfoFormat("Handled {0}", typeof(T).Name);
        }
        
        public void Dispose()
        {
           
        }
    }
}