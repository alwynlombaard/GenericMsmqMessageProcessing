using System;
using AutoMoq;
using GenericMsmqProcessing.Core;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.MessageProccessor;
using GenericMsmqProcessing.Core.Queue;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace GenericMessageProccessing.Core.Test.Unit
{
    [TestFixture]
    [Category("Fast")]
    public class MessageProcessorCollectionFactoryTests
    {
        public class FakeHandler<T> : IMessageHandler<T>
        {
            public void HandleMessage(T message) { }
            public void OnError(T message, Exception ex) { }
            public void Dispose() { }
        }

        public class FakeMessageHandler : IMessageHandler<FakeMessageWithHandler>
        {
            public void HandleMessage(FakeMessageWithHandler message) { }
            public void OnError(FakeMessageWithHandler message, Exception ex) { }
            public void Dispose() { }
        }

        public class FakeMessageHandler2 : IMessageHandler<FakeMessage2WithHandler>
        {
            public void HandleMessage(FakeMessage2WithHandler message) { }
            public void OnError(FakeMessage2WithHandler message, Exception ex) { }
            public void Dispose() { }
        }

        public class FakeMessage2Handler : IMessageHandler<FakeMessage2WithHandler>
        {
            public void HandleMessage(FakeMessage2WithHandler message) { }
            public void OnError(FakeMessage2WithHandler message, Exception ex) { }
            public void Dispose() { }
        }

        public class FakeMessage2Handler2 : IMessageHandler<FakeMessage2WithHandler>
        {
            public void HandleMessage(FakeMessage2WithHandler message) { }
            public void OnError(FakeMessage2WithHandler message, Exception ex) { }
            public void Dispose() { }
        }

        public class FakeQueue<T> : IMessageQueueInbound<T>
        {
            public bool TryReceive(out T message)
            {
                message = default(T);
                return true;
            }
        }

        private AutoMoqer _mocker;
        private Func<Type, object> _serviceLocator;
  
        [SetUp]
        public void SetUp()
        {
            var testContainer = new UnityContainer()
                .RegisterType(typeof(IMessageHandler<>), typeof(FakeHandler<>))
                .RegisterInstance(typeof(IMessageHandler<FakeMessageWithHandler>), new FakeMessageHandler())
                .RegisterInstance(typeof(IMessageHandler<FakeMessage2WithHandler>), new FakeMessage2Handler())
                .RegisterType(typeof(IMessageQueueInbound<>), typeof(FakeQueue<>));

            _mocker = new AutoMoqer(testContainer);
  
            _serviceLocator = type => _mocker.Create(type);
        }


        public struct FakeMessageWithHandler : IMessage { }
        public struct FakeMessage2WithHandler : IMessage { }

        [Test]
        public void CollectionCanManufactureCollection()
        {
            var processors = MessageProcessorCollectionFactory.Collection(_serviceLocator);

            Assert.That(processors, Has.Some.AssignableFrom(typeof(MessageProcessor<FakeMessageWithHandler>)));
            Assert.That(processors, Has.Some.AssignableFrom(typeof(MessageProcessor<FakeMessage2WithHandler>)));
        }

        [Test]
        public void CollectionCreatedOnceOnly()
        {
            var processors = MessageProcessorCollectionFactory.Collection(_serviceLocator);
            var processors2 = MessageProcessorCollectionFactory.Collection(_serviceLocator);
            var processors3 = MessageProcessorCollectionFactory.Collection();

            Assert.That(processors2, Is.SameAs(processors));
            Assert.That(processors3, Is.SameAs(processors));
        }


        public struct FakeMessageWithoutAnyHandlers : IMessage { }

        [Test]
        public void CollectionNotCreatedForMessagesWithoutHandlers()
        {
            var processors = MessageProcessorCollectionFactory.Collection(_serviceLocator);

            Assert.That(processors, Has.None.AssignableFrom(typeof(MessageProcessor<FakeMessageWithoutAnyHandlers>)));

            Assert.That(processors, Has.Some.AssignableFrom(typeof(MessageProcessor<FakeMessageWithHandler>)));
            Assert.That(processors, Has.Some.AssignableFrom(typeof(MessageProcessor<FakeMessage2WithHandler>)));
        }
    }
}
