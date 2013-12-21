using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using GenericMsmqProcessing.Core;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.MessageProccessor;
using GenericMsmqProcessing.Core.Queue;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;

namespace GenericMessageProccessing.Core.Test.Unit
{
    [TestFixture]
    [Category("Fast")]
    public class MessageProcessorTests
    {
        public struct FakeMessage : IMessage
        {

        }

        private Mock<IMessageQueueInbound<FakeMessage>> _messageQueue;
        private Mock<IMessageHandler<FakeMessage>> _messageHandler;
        private Mock<IMessageHandler<FakeMessage>> _messageHandler2;
        private FakeMessage _fakeMessage;

        [SetUp]
        public void SetUp()
        {
            _messageQueue = new Mock<IMessageQueueInbound<FakeMessage>>();
            _messageHandler = new Mock<IMessageHandler<FakeMessage>>();
            _messageHandler2 = new Mock<IMessageHandler<FakeMessage>>();
            _fakeMessage = new FakeMessage();
        }

        [Test]
        public void MessageProcessorHandlesInboundMessage()
        {

            _messageQueue.Setup(x => x.TryReceive(out _fakeMessage))
               .ReturnsInOrder(false, true, false);

            var processor = new MessageProcessor<FakeMessage>(_messageQueue.Object, _messageHandler.Object);
            processor.Start();
            Thread.Sleep(100);
            processor.Stop();

            _messageHandler.Verify(h => h.HandleMessage(_fakeMessage), Times.Once());
        }

        [Test]
        public void MessageProcessorWithMultipleHandlersHandlesInboundMessage()
        {

            _messageQueue.Setup(x => x.TryReceive(out _fakeMessage))
               .ReturnsInOrder(false, true, false);

            var handlers = new List<IMessageHandler<FakeMessage>>
            {
                _messageHandler.Object, 
                _messageHandler2.Object
            };

            var processor = new MessageProcessor<FakeMessage>(_messageQueue.Object, handlers);

            processor.Start();
            Thread.Sleep(100);
            processor.Stop();

            _messageHandler.Verify(h => h.HandleMessage(_fakeMessage), Times.Once());
            _messageHandler2.Verify(h => h.HandleMessage(_fakeMessage), Times.Once());
        }

        [Test]
        public void MessageProcessorHandlesMultipleInboundMessages()
        {

            _messageQueue.Setup(x => x.TryReceive(out _fakeMessage))
               .ReturnsInOrder(true, true, false, true, false);

            var processor = new MessageProcessor<FakeMessage>(_messageQueue.Object, _messageHandler.Object);
            processor.Start();
            Thread.Sleep(500);
            processor.Stop();

            _messageHandler.Verify(h => h.HandleMessage(_fakeMessage), Times.Exactly(3));
        }

        [Test]
        public void MessageProcessorWithMultipleHandlersHandlesInboundMessages()
        {

            _messageQueue.Setup(x => x.TryReceive(out _fakeMessage))
              .ReturnsInOrder(true, true, false, true, false);

            var handlers = new List<IMessageHandler<FakeMessage>>
            {
                _messageHandler.Object, 
                _messageHandler2.Object
            };

            var processor = new MessageProcessor<FakeMessage>(_messageQueue.Object, handlers);

            processor.Start();
            Thread.Sleep(100);
            processor.Stop();

            _messageHandler.Verify(h => h.HandleMessage(_fakeMessage), Times.Exactly(3));
            _messageHandler2.Verify(h => h.HandleMessage(_fakeMessage), Times.Exactly(3));
        }

        [Test]
        public void MessageProcessorCanStopAndRestart()
        {

            _messageQueue.Setup(x => x.TryReceive(out _fakeMessage))
               .ReturnsInOrder(true);

            var processor = new MessageProcessor<FakeMessage>(_messageQueue.Object, _messageHandler.Object);
            processor.Start();
            Thread.Sleep(500);
            processor.Stop();

            _messageHandler.Verify(h => h.HandleMessage(_fakeMessage), Times.Once());

            _messageQueue.Setup(x => x.TryReceive(out _fakeMessage))
              .ReturnsInOrder(true);

            processor.Start();
            Thread.Sleep(500);
            processor.Stop();

            _messageHandler.Verify(h => h.HandleMessage(_fakeMessage), Times.Exactly(2));
        }

        [Test]
        public void MessageProcessorIncrementsNumberOfMessagesPickedUp()
        {
            _messageQueue.Setup(x => x.TryReceive(out _fakeMessage))
              .ReturnsInOrder(true, false, true, false, true, false, false, false);

            var processor = new MessageProcessor<FakeMessage>(_messageQueue.Object, _messageHandler.Object);

            Assert.That(processor.NumberOfMessagesPickedUp, Is.EqualTo(0));

            processor.Start();
            Thread.Sleep(500);
            processor.Stop();

            Assert.That(processor.NumberOfMessagesPickedUp, Is.EqualTo(3));
        }

        [Test]
        public void MessageProcessorIsRunningIsSetCorrectly()
        {
            var processor1 = new MessageProcessor<FakeMessage>(_messageQueue.Object, _messageHandler.Object);
            var processor2 = new MessageProcessor<FakeMessage>(_messageQueue.Object, _messageHandler.Object);

            Assert.That(processor1.IsRunning(), Is.False);
            Assert.That(processor2.IsRunning(), Is.False);

            processor1.Start();

            Assert.That(processor1.IsRunning(), Is.True);
            Assert.That(processor2.IsRunning(), Is.False);

            processor2.Start();

            Assert.That(processor1.IsRunning(), Is.True);
            Assert.That(processor2.IsRunning(), Is.True);

            processor1.Stop();

            Assert.That(processor1.IsRunning(), Is.False);
            Assert.That(processor2.IsRunning(), Is.True);

            processor2.Stop();

            Assert.That(processor1.IsRunning(), Is.False);
            Assert.That(processor2.IsRunning(), Is.False);
        }

        [Test]
        public void MessageProcessorInvokesHandlerOnErrorWhenHandlerThrows()
        {
            _messageQueue.Setup(x => x.TryReceive(out _fakeMessage))
             .ReturnsInOrder(true);

            _messageHandler.Setup(h => h.HandleMessage(It.IsAny<FakeMessage>())).Throws(new Exception("hello error"));

            var processor = new MessageProcessor<FakeMessage>(_messageQueue.Object, _messageHandler.Object);
            processor.Start();
            Thread.Sleep(500);
            processor.Stop();

            _messageHandler.Verify(h => h.OnError(_fakeMessage, It.Is<Exception>(ex => ex.Message == "hello error")), Times.Once());

        }

        [Test]
        public void MessageProcessorIncrementsNumberOfMessageErrors()
        {
            _messageQueue.Setup(x => x.TryReceive(out _fakeMessage))
            .ReturnsInOrder(true);

            _messageHandler.Setup(h => h.HandleMessage(It.IsAny<FakeMessage>())).Throws<Exception>();

            var processor = new MessageProcessor<FakeMessage>(_messageQueue.Object, _messageHandler.Object);

            Assert.That(processor.NumberOfMessageErrors, Is.EqualTo(0));

            processor.Start();
            Thread.Sleep(500);
            processor.Stop();

            Assert.That(processor.NumberOfMessageErrors, Is.EqualTo(1));

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
                return (TResult)result;
            });
        }
    }
}
