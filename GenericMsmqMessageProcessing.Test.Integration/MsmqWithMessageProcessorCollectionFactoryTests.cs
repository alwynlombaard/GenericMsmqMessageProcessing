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
    public class MsmqWithMessageProcessorCollectionFactoryTests
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
        private Mock<ILog> _logger;
        private IMessageProccessorCollection _messageProcessorCollection;
        private static Mock<IDoSomeWorkWithMyMessage> _iDoSomeWorkWithMyMessage;
            
        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMoqer();
            _logger = _mocker.GetMock<ILog>();
            _iDoSomeWorkWithMyMessage = _mocker.GetMock<IDoSomeWorkWithMyMessage>();

            _messageProcessorCollection = new MessageProcessorCollectionFactory(_logger.Object).Manufacture();

            TestQueues.PurgeQueues();

            _messageProcessorCollection.StartAll();
        }

       

        [TearDown]
        public void TearDown()
        {
            _messageProcessorCollection.StopAll();
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
