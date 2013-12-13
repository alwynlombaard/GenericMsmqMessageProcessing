using System;
using System.Threading;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.MessageProccessor;
using GenericMsmqProcessing.Core.Queue.Msmq;
using log4net;
using Moq;
using NUnit.Framework;
using AutoMoq;

namespace GenericMsmqMessageProcessing.Test.Integration
{
    [TestFixture]
    [Category("Slow")]
    public class MsmqMessageQueueTests
    {
        class MyMessageHandler : IMessageHandler <MyMessage>
        {
            public void HandleMessage(MyMessage message)
            {
                _iDoSomeWorkWithMyMessage.Object.DoWork(message);
            }

            public void OnError(MyMessage message, Exception ex)
            {
                
            }

            public void Dispose()
            {
               
            }
        }

        private AutoMoqer _mocker;
        private MyMessageHandler _messageHandler;
        private Mock<ILog> _logger;
        private MsmqMessageQueueInbound<MyMessage> _inboundMessageQueue;
        private MessageProcessor<MyMessage> _messageProcessor;
        private static Mock<IDoSomeWorkWithMyMessage> _iDoSomeWorkWithMyMessage;
            
        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMoqer();
            _logger = _mocker.GetMock<ILog>();
            _iDoSomeWorkWithMyMessage = _mocker.GetMock<IDoSomeWorkWithMyMessage>();
          
            _messageHandler = new MyMessageHandler();
            _inboundMessageQueue = new MsmqMessageQueueInbound<MyMessage>(_logger.Object);
            _messageProcessor = new MessageProcessor<MyMessage>(_inboundMessageQueue,
                _messageHandler);

            TestQueues.PurgeQueues();

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
            var queue = new MsmqMessageQueueOutbound<MyMessage>();
            
            var message = new MyMessage{Message = "Hello"};
            queue.Send(message);

            var message2 = new MyMessage {Message = "Hello 2"};
            queue.Send(message2);

            Thread.Sleep(1000);
            
            _iDoSomeWorkWithMyMessage.Verify(x=> x.DoWork(It.Is<MyMessage>(m => m.Message == "Hello")), Times.Once());
            _iDoSomeWorkWithMyMessage.Verify(x=> x.DoWork(It.Is<MyMessage>(m => m.Message == "Hello 2")), Times.Once());

        }
    }
}
