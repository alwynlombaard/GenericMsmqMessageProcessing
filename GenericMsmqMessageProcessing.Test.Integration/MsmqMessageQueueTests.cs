using System;
using System.Threading;
using GenericMsmqProcessing.Core;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.MessageProccessor;
using GenericMsmqProcessing.Core.Queue.Msmq;
using log4net;
using Moq;
using NUnit.Framework;
using AutoMoq;

namespace GenericMsmqMessageProcessing.Test.Integration
{
    [Serializable]
    public struct MyMessageForMsmqMessageQueueTests : IMessage
    {
        public string Message { get; set; }
    }

    [TestFixture]
    [Category("Slow")]
    public class MsmqMessageQueueTests
    {
        public class MyMessageHandler : IMessageHandler<MyMessageForMsmqMessageQueueTests>
        {
            public void HandleMessage(MyMessageForMsmqMessageQueueTests message)
            {
                _iDoSomeWorkWithMyMessage.Object.DoWork(message);
            }

            public void OnError(MyMessageForMsmqMessageQueueTests message, Exception ex)
            {

            }

            public void Dispose()
            {

            }
        }

        private AutoMoqer _mocker;
        private MyMessageHandler _messageHandler;
        private Mock<ILog> _logger;
        private MsmqMessageQueueInbound<MyMessageForMsmqMessageQueueTests> _inboundMessageQueue;
        private IMessageProcessor _messageProcessor;
        private static Mock<IDoSomeWorkWithIMessage> _iDoSomeWorkWithMyMessage;

        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMoqer();
            _logger = _mocker.GetMock<ILog>();
            _iDoSomeWorkWithMyMessage = _mocker.GetMock<IDoSomeWorkWithIMessage>();

            _messageHandler = new MyMessageHandler();
            _inboundMessageQueue = new MsmqMessageQueueInbound<MyMessageForMsmqMessageQueueTests>(_logger.Object);
            _messageProcessor = new MessageProcessor<MyMessageForMsmqMessageQueueTests>(_inboundMessageQueue,
                _messageHandler);

            TestQueues.PurgeQueues(typeof(MyMessageForMsmqMessageQueueTests));

            _messageProcessor.Start();
        }



        [TearDown]
        public void TearDown()
        {
            _messageProcessor.Stop();
        }

        [Test]
        public void CanHandleMessages()
        {
            var queue = new MsmqMessageQueueOutbound<MyMessageForMsmqMessageQueueTests>();

            var message = new MyMessageForMsmqMessageQueueTests { Message = "Hello" };
            queue.Send(message);

            var message2 = new MyMessageForMsmqMessageQueueTests { Message = "Hello 2" };
            queue.Send(message2);

            Thread.Sleep(1000);

            _iDoSomeWorkWithMyMessage.Verify(x => x.DoWork(It.Is<MyMessageForMsmqMessageQueueTests>(m => m.Message == "Hello")), Times.Once());
            _iDoSomeWorkWithMyMessage.Verify(x => x.DoWork(It.Is<MyMessageForMsmqMessageQueueTests>(m => m.Message == "Hello 2")), Times.Once());

        }
    }
}
