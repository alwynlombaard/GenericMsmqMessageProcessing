using System;
using System.Threading;
using GenericMsmqProcessing.Core;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.MessageProccessor;
using GenericMsmqProcessing.Core.Queue.Msmq;
using log4net;
using Moq;
using NUnit.Framework;
using ReallySimpleEventing;
using ReallySimpleEventing.EventHandling;
using AutoMoq;

namespace GenericMsmqMessageProcessing.Test.Integration
{

    public class MyMessage : IMessage
    {
    }

    public interface IDoSomeWorkWithMyMessage
    {
        void DoWork(MyMessage message);
    }

    [TestFixture]
    public class MsmqMessageQueueTests
    {
        internal class MyHandler : IHandle<MyMessage>
        {
            public void Handle(MyMessage message)
            {
                _iDoSomeWorkWithMyMessage.Object.DoWork(message);
            }

            public void OnError(MyMessage message, Exception ex)
            {

            }
        }

        private AutoMoqer _mocker;
        private MessageHandler<MyMessage> _messageHandler;
        private Mock<ILog> _logger;
        private IEventStream _eventStream;
        private MsmqMessageQueueInbound<MyMessage> _inboundMessageQueue;
        private MessageProcessor<MyMessage> _messageProcessor;
        private static Mock<IDoSomeWorkWithMyMessage> _iDoSomeWorkWithMyMessage;
            
        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMoqer();
            _logger = _mocker.GetMock<ILog>();
            _iDoSomeWorkWithMyMessage = _mocker.GetMock<IDoSomeWorkWithMyMessage>();

            _eventStream = ReallySimpleEventing.ReallySimpleEventing.CreateEventStream();
            _messageHandler = new MessageHandler<MyMessage>(_eventStream);
            _inboundMessageQueue = new MsmqMessageQueueInbound<MyMessage>(_logger.Object);
            _messageProcessor = new MessageProcessor<MyMessage>(_logger.Object,
                _inboundMessageQueue,
                _messageHandler);

            _messageProcessor.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _messageProcessor.Stop();
        }

        [Test]
        public void CanHandleMessage()
        {
            var queue = new MsmqMessageQueueOutbound<MyMessage>();
            var message = new MyMessage();
            queue.Send(message);

            Thread.Sleep(1000);

            _iDoSomeWorkWithMyMessage.Verify(x=> x.DoWork(It.IsAny<MyMessage>()), Times.Once());

        }
    }
}
