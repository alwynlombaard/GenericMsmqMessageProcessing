using System;
using System.Messaging;
using log4net;

namespace GenericMsmqProcessing.Core.Queue.Msmq
{
    public class MsmqMessageQueueInbound<T> : IMessageQueueInbound<T>
    {
        private readonly ILog _log;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5.0);
        private readonly IMessageFormatter _formatter = new BinaryMessageFormatter();

        private readonly string _path;

        public MsmqMessageQueueInbound(ILog log)
        {
            _log = log;
            _path = @".\private$\" + typeof(T).FullName;
            if (!MessageQueue.Exists(_path))
            {
                MessageQueue.Create(_path, transactional: true);
            }
        }

        public bool TryReceive(out T message)
        {
            try
            {
                using (var queue = new MessageQueue(_path))
                {
                    var msmqMessage = queue.Receive(_timeout, MessageQueueTransactionType.Automatic);
                    if (msmqMessage == null)
                    {
                        message = default(T);
                        return false;
                    }
                    msmqMessage.Formatter = _formatter;
                    message = (T) msmqMessage.Body;
                    return true;
                }
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                {
                    _log.Error("MessageQueue receiving error", ex);    
                }
                message = default(T);
                return false;
            }
            catch(Exception ex)
            {
                _log.Error(typeof(T).Name +  ": Error receiving message ", ex);    
                message = default(T);
                return false;
            }
        }
    }
}
