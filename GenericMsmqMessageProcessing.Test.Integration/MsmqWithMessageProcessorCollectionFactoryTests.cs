using System;
using System.Threading;
using GenericMsmqProcessing.Core;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.MessageProccessor;
using GenericMsmqProcessing.Core.Queue.Msmq;
using Moq;
using NUnit.Framework;

namespace GenericMsmqMessageProcessing.Test.Integration
{
    [Serializable]
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

        class MyMessageHandler2 : IMessageHandler<MyMessageForFactoryTesting>
        {
            public void HandleMessage(MyMessageForFactoryTesting message)
            {
                _iAlsoDoSomeWorkWithMyMessage.Object.DoWork(message);
            }

            public void OnError(MyMessageForFactoryTesting message, Exception ex)
            {

            }

            public void Dispose()
            {

            }
        }

        private IMessageProcessorCollection _messageProcessorCollection;
        private static Mock<IDoSomeWorkWithIMessage> _iDoSomeWorkWithMyMessage;
        private static Mock<IDoSomeWorkWithIMessage> _iAlsoDoSomeWorkWithMyMessage;

        [SetUp]
        public void SetUp()
        {
            _iDoSomeWorkWithMyMessage = new Mock<IDoSomeWorkWithIMessage>();
            _iAlsoDoSomeWorkWithMyMessage = new Mock<IDoSomeWorkWithIMessage>();

            _messageProcessorCollection = MessageProcessorCollectionFactory.Collection();

            TestQueues.PurgeQueues(typeof(MyMessageForFactoryTesting));

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

            _iAlsoDoSomeWorkWithMyMessage.Verify(x => x.DoWork(It.Is<MyMessageForFactoryTesting>(m => m.Message == "Hello")), Times.Once());
            _iAlsoDoSomeWorkWithMyMessage.Verify(x => x.DoWork(It.Is<MyMessageForFactoryTesting>(m => m.Message == "Hello 2")), Times.Once());

        }
    }
}
