using System;
using System.Collections;
using System.Threading;
using AutoMoq;
using GenericMsmqProcessing.Core;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.MessageProccessor;
using GenericMsmqProcessing.Core.Queue;
using log4net;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;

namespace GenericMessageProccessing.Core.Test.Unit
{

   

    [TestFixture]
    [Category("Fast")]
    public class MessageProcessorTests
    {
        public class FakeAnalyticsMessage : IMessage
        {

        }
        private AutoMoqer _mocker;
        private Func<Type, object> _serviceLocator;
        private Mock<IMessageQueueInbound<FakeAnalyticsMessage>> _messageQueue;
        private Mock<IMessageHandler<FakeAnalyticsMessage>> _messageHandler;
        private FakeAnalyticsMessage _fakeMessage;
        private Mock<ILog> _log;

        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMoqer();
            _messageQueue = new Mock<IMessageQueueInbound<FakeAnalyticsMessage>>();
            _messageHandler = _mocker.GetMock<IMessageHandler<FakeAnalyticsMessage>>();
            _fakeMessage = new FakeAnalyticsMessage();
            _log = _mocker.GetMock<ILog>();

            _serviceLocator = type =>
            {
                if (type == typeof(IMessageHandler<FakeAnalyticsMessage>))
                {
                    return _messageHandler.Object;
                }
                if (type == typeof(IMessageQueueInbound<FakeAnalyticsMessage>))
                {
                    return _messageQueue.Object;    
                }
                if (type == typeof(ILog))
                {
                    return _log.Object;
                }
                return new {};
            };
        }


        [Test]
        public void MessageProcessorWithServiceLocatorHandlesInboundMessage()
        {

            _messageQueue.Setup(x => x.TryReceive(out _fakeMessage))
               .ReturnsInOrder(false, true, false);

            var msmqProcessor = new MessageProcessor<FakeAnalyticsMessage>(_serviceLocator);
            msmqProcessor.Start();
            Thread.Sleep(100);
            msmqProcessor.Stop();

            _messageHandler.Verify(h => h.HandleMessage(_fakeMessage), Times.Once());
            
        }

        [Test]
        public void MessageProcessorHandlesInboundMessage()
        {

            _messageQueue.Setup(x => x.TryReceive(out _fakeMessage))
               .ReturnsInOrder(false, true, false);

            var msmqProcessor = new MessageProcessor<FakeAnalyticsMessage>(_log.Object, _messageQueue.Object, _messageHandler.Object);
            msmqProcessor.Start();
            Thread.Sleep(100);
            msmqProcessor.Stop();

            _messageHandler.Verify(h => h.HandleMessage(_fakeMessage), Times.Once());

        }


        [Test]
        public void MsmqMessageProcessorHandlesMultipleInboundMessages()
        {

            _messageQueue.Setup(x => x.TryReceive(out _fakeMessage))
               .ReturnsInOrder(true, true, false, true, false);

            var msmqProcessor = new MessageProcessor<FakeAnalyticsMessage>(_serviceLocator);
            msmqProcessor.Start();
            Thread.Sleep(100);
            msmqProcessor.Stop();

            _messageHandler.Verify(h => h.HandleMessage(_fakeMessage), Times.Exactly(3));

        }

        [Test]
        public void MsmqMessageProcessorLogsErrors()
        {
            _messageQueue.Setup(x => x.TryReceive(out _fakeMessage))
                .ReturnsInOrder(true, new Exception(), true);
            
            var msmqProcessor = new MessageProcessor<FakeAnalyticsMessage>(_serviceLocator);
            msmqProcessor.Start();
            Thread.Sleep(250);
            msmqProcessor.Stop();

            _messageHandler.Verify(h => h.HandleMessage(_fakeMessage), Times.Exactly(2));
            _log.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once());
        }

    }

    public static class TestExtensions
    {
        public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup,
            params object[] results) where T : class
        {
            var queue = new Queue(results);
            setup.Returns(() =>
            {
                if (queue.Count == 0)
                {
                    return default(TResult);
                }
                var result = queue.Dequeue();
                if (result is Exception)
                {
                    throw result as Exception;
                }
                return (TResult) result;
            });    
        }
    }
}
