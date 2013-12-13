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

    public struct MyMessageForFactoryTesting : IMessage
    {
        public string Message { get; set; }
    }


    [TestFixture]
    [Category("Slow")]
    public class MsmqWithMessageProcessorCollectionFactoryTests
    {
        class MyMessageHandler : IMessageHandler<MyMessageForFactoryTesting>
        {
            public void HandleMessage(MyMessageForFactoryTesting message)
            {
                _iDoSomeWorkWithMyMessage.Object.DoWork(message);
            }

            public void OnError(MyMessageForFactoryTesting message, Exception ex)
            {
                
            }

            public void Dispose()
            {
               
            }
        }

        private AutoMoqer _mocker;
        private Mock<ILog> _logger;
        private IMessageProccessorCollection _messageProcessorCollection;
        private static Mock<IDoSomeWorkWithIMessage> _iDoSomeWorkWithMyMessage;
            
        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMoqer();
            _logger = _mocker.GetMock<ILog>();
            _iDoSomeWorkWithMyMessage = _mocker.GetMock<IDoSomeWorkWithIMessage>();

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
            var queue = new MsmqMessageQueueOutbound<MyMessageForFactoryTesting>();

            var message = new MyMessageForFactoryTesting { Message = "Hello" };
            queue.Send(message);

            var message2 = new MyMessageForFactoryTesting { Message = "Hello 2" };
            queue.Send(message2);
           
            Thread.Sleep(1000);

            _iDoSomeWorkWithMyMessage.Verify(x => x.DoWork(It.Is<MyMessageForFactoryTesting>(m => m.Message == "Hello")), Times.Once());
            _iDoSomeWorkWithMyMessage.Verify(x => x.DoWork(It.Is<MyMessageForFactoryTesting>(m => m.Message == "Hello 2")), Times.Once());

        }
    }
}
