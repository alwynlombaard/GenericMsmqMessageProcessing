using System;
using System.Threading;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.Queue;

namespace GenericMsmqProcessing.Core.MessageProccessor
{
    public class MessageProcessor<T> : IMessageProcessor<T> where T : IMessage
    {
        private readonly IMessageQueueInbound<T> _messageQueue;

        private ManualResetEvent _stop = new ManualResetEvent(false);
        private Thread _thread;
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
            if (_thread.ThreadState == ThreadState.Stopped)
            {
                _thread = null;
                _stop = null;
                _thread = new Thread(ThreadProc) { Name = GetType().FullName };
            }
            if (_thread.ThreadState == ThreadState.Unstarted)
            {
                _stop = new ManualResetEvent(false);
                _thread.Start();
            }
        }

        public void Stop()
        {
            if (IsRunning())
            {
                _stop.Set();
                _thread.Join();
            }
        }

        public bool IsRunning()
        {
            return _thread.ThreadState != ThreadState.Unstarted
                && _thread.ThreadState != ThreadState.Stopped;
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
