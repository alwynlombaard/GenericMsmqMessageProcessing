using System.Messaging;

namespace GenericMsmqProcessing.Core.Queue.Msmq
{
    public class MsmqMessageQueueOutbound<T> : IMessageQueueOutbound<T>
    {
        private readonly XmlMessageFormatter _formatter = new XmlMessageFormatter(new[] { typeof(T) });

        private readonly string _path;

        public MsmqMessageQueueOutbound()
        {
            _path = @".\private$\" + typeof(T).FullName;
            if (!MessageQueue.Exists(_path))
            {
                MessageQueue.Create(_path, transactional: false);
            }
        }

        public void Send(T message)
        {

            using (var queue = new MessageQueue(_path))
            {
                queue.DefaultPropertiesToSend.Recoverable = true;
                queue.Formatter = _formatter;
                queue.Send(message, MessageQueueTransactionType.Automatic);
            }
        }
    }
}
