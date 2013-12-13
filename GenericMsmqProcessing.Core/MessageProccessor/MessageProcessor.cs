using System;
using System.Threading;
using System.Threading.Tasks;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.Queue;

namespace GenericMsmqProcessing.Core.MessageProccessor
{
    public class MessageProcessor<T> : IMessageProcessor<T> where T : IMessage
    {
        private readonly object _lock = new object();
        private bool _running;
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly IMessageQueueInbound<T> _messageQueue;
        private readonly IMessageHandler<T> _messageHandler;

       public MessageProcessor(IMessageQueueInbound<T> messageQueue, IMessageHandler<T> messageHandler)
        {
            _messageQueue = messageQueue;
            _messageHandler = messageHandler;
            Name = typeof(T).Name;
        }

        private void ListenForMessages()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
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

        public void Start()
        {
            lock (_lock)
            {
                if (_running) return;
                _cancellationTokenSource = new CancellationTokenSource();
                _task = new Task(ListenForMessages, _cancellationTokenSource.Token);
                _task.Start();
                _running = true;
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (!_running) return;
                _cancellationTokenSource.Cancel();
                _running = false;
            }
        }

        public string Name { get; set; }
    }
}