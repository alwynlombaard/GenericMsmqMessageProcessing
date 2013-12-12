using System;
using System.Threading;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.Queue;
using log4net;

namespace GenericMsmqProcessing.Core.MessageProccessor
{
    public class MessageProcessor<T> : IMessageProcessor<T> where T : IMessage
    {
        private readonly IMessageQueueInbound<T> _messageQueue;

        private readonly ManualResetEvent _stop = new ManualResetEvent(false);
        private readonly Thread _thread;
        private readonly IMessageHandler<T> _messageHandler;


        public MessageProcessor(IMessageQueueInbound<T> messageQueue, IMessageHandler<T> messageHandler)
        {
            _thread = new Thread(ThreadProc) { Name = GetType().FullName };
            _messageQueue = messageQueue;
            _messageHandler = messageHandler;
            Name = typeof(T).Name;
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
                T message;
                if (!_messageQueue.TryReceive(out message))
                {
                    continue;
                }
                using (_messageHandler)
                {
                    try
                    {
                        _messageHandler.HandleMessage(message);
                    }
                    catch (Exception handlerException)
                    {
                        _messageHandler.OnError(message, handlerException);
                    }
                }
            }
        }

        public string Name { get; set; }
    }
}
