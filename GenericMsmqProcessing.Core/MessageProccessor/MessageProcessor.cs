using System;
using System.Threading;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.Queue;
using log4net;

namespace GenericMsmqProcessing.Core.MessageProccessor
{
    public class MessageProcessor<T> : IMessageProcessor<T> where T : IMessage
    {
        private readonly ILog _log;
        private readonly IMessageQueueInbound<T> _messageQueue;

        private readonly ManualResetEvent _stop = new ManualResetEvent(false);
        private readonly Thread _thread;
        private readonly IMessageHandler<T> _messageHandler;

        public MessageProcessor()
        {
            _thread = new Thread(ThreadProc) { Name = GetType().FullName };
            Name = typeof(T).Name;
        }

        public MessageProcessor(ILog log, IMessageQueueInbound<T> messageQueue, IMessageHandler<T> messageHandler ) :this()
        {
            _log = log;
            _messageQueue = messageQueue;
            _messageHandler = messageHandler;
        }

        public MessageProcessor(Func<Type, object> serviceCreator) : this()
        {
            _log = (ILog) serviceCreator(typeof (ILog));
            _messageHandler = (IMessageHandler<T>) serviceCreator(typeof (IMessageHandler<T>));
            _messageQueue = (IMessageQueueInbound<T>) serviceCreator(typeof (IMessageQueueInbound<T>));
        }

        public void Start()
        {
            if (_thread.ThreadState == ThreadState.Unstarted)
            {
                _thread.Start();
            }
        }

        public void Stop()
        {
            _stop.Set();
            _thread.Join();
        }

        private void ThreadProc(object o)
        {
            while (!_stop.WaitOne(0))
            {
                try
                {
                    T message;
                    if (!_messageQueue.TryReceive(out message))
                    {
                        continue;
                    }
                    using (_messageHandler)
                    {
                        _messageHandler.HandleMessage(message);
                    }
                }
                catch(Exception ex)
                {
                    _log.Error(typeof(T).Name +  ": error while handling message" , ex);
                }
            }
        }

        public string Name { get; set; }
    }
}
