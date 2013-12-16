using System;
using AutoMoq;
using GenericMsmqProcessing.Core;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.MessageProccessor;
using GenericMsmqProcessing.Core.Queue;
using log4net;
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
            public void HandleMessage(T message){}
            public void OnError(T message, Exception ex){}
            public void Dispose(){}
        }

        public class FakeMessageHandler : IMessageHandler<FakeMessage>
        {
            public void HandleMessage(FakeMessage message) { }
            public void OnError(FakeMessage message, Exception ex) { }
            public void Dispose() { }
        }
        
        public class FakeMessageHandler2 : IMessageHandler<FakeMessage>
        {
            public void HandleMessage(FakeMessage message) { }
            public void OnError(FakeMessage message, Exception ex) { }
            public void Dispose() { }
        }

        public class FakeMessage2Handler : IMessageHandler<FakeMessage2>
        {
            public void HandleMessage(FakeMessage2 message) { }
            public void OnError(FakeMessage2 message, Exception ex) { }
            public void Dispose() { }
        }

        public class FakeMessage2Handler2 : IMessageHandler<FakeMessage2>
        {
            public void HandleMessage(FakeMessage2 message) { }
            public void OnError(FakeMessage2 message, Exception ex) { }
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
                .RegisterType(typeof (IMessageHandler<>), typeof (FakeHandler<>))
                .RegisterInstance(typeof (IMessageHandler<FakeMessage>), new FakeMessageHandler())
                .RegisterInstance(typeof (IMessageHandler<FakeMessage2>), new FakeMessage2Handler())
                .RegisterType(typeof (IMessageQueueInbound<>), typeof (FakeQueue<>));

            _mocker = new AutoMoqer(testContainer);
            _mocker.GetMock<ILog>();

            _serviceLocator = type => _mocker.Create(type);
        }


        public struct FakeMessage : IMessage { }
        public struct FakeMessage2 : IMessage { }

        [Test]
        public void ManafactureCanManufactureCollection()
        {
            var processors = MessageProcessorCollectionFactory.Collection(_serviceLocator);

            Assert.That(processors, Has.Some.AssignableFrom(typeof (MessageProcessor<FakeMessage>)));
            Assert.That(processors, Has.Some.AssignableFrom(typeof (MessageProcessor<FakeMessage2>)));
        }
    }
}
