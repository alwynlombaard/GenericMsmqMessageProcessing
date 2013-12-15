using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.Queue;

namespace GenericMsmqProcessing.Core.MessageProccessor
{
    public sealed class MessageProcessor<T> : IMessageProcessor<T> where T : IMessage
    {
        private readonly object _lock = new object();
        private bool _running;
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly IMessageQueueInbound<T> _messageQueue;
        private readonly List<IMessageHandler<T>> _messageHandlers;


        public static IMessageProcessor Manufacture(IMessageQueueInbound<T> messageQueue,
            IEnumerable<IMessageHandler<T>> messageHandlers)
        {
            return new MessageProcessor<T>(messageQueue, messageHandlers);
        }

        public static IMessageProcessor Manufacture(IMessageQueueInbound<T> messageQueue,
            IMessageHandler<T> messageHandler)
        {
            return new MessageProcessor<T>(messageQueue, messageHandler);
        }

        private MessageProcessor(IMessageQueueInbound<T> messageQueue, IEnumerable<IMessageHandler<T>> messageHandlers)
        {
            _messageQueue = messageQueue;
            _messageHandlers = Enumerable.Empty<IMessageHandler<T>>().ToList();
            _messageHandlers.AddRange(messageHandlers);
            Name = typeof(T).Name;
        }
        
        
        private MessageProcessor(IMessageQueueInbound<T> messageQueue, IMessageHandler<T> messageHandler)
        {
            _messageQueue = messageQueue;
            _messageHandlers = Enumerable.Empty<IMessageHandler<T>>().ToList();
            _messageHandlers.Add(messageHandler);
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
                foreach (var messageHandler in _messageHandlers)
                {
                    using (messageHandler)
                    {
                        try
                        {
                            messageHandler.HandleMessage(message);
                        }
                        catch (Exception handlerException)
                        {
                            messageHandler.OnError(message, handlerException);
                        }
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