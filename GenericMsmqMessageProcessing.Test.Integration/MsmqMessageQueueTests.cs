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



    [TestFixture]
    public class MsmqMessageQueueTests
    {

        private AutoMoqer _mocker;
        private MessageHandler<MyMessage> _messageHandler;
        private Mock<ILog> _logger;
        private Mock<IEventStream> _eventStream;
        private MsmqMessageQueueInbound<MyMessage> _inboundMessageQueue;
        private MessageProcessor<MyMessage> _messageProcessor;

        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMoqer();
            _logger = _mocker.GetMock<ILog>();
            _eventStream = _mocker.GetMock<IEventStream>();

            _messageHandler = new MessageHandler<MyMessage>(_eventStream.Object);
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

            _eventStream.Verify(x=> x.Raise(It.IsAny<MyMessage>()), Times.Once());

        }
    }
}
