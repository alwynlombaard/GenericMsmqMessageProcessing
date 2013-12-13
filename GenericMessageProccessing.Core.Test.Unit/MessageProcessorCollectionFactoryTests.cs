using System;
using AutoMoq;
using GenericMsmqProcessing.Core;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.MessageProccessor;
using GenericMsmqProcessing.Core.Queue;
using log4net;
using Microsoft.Practices.Unity;
using Moq;
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

        public class FakeQueue<T> : IMessageQueueInbound<T>
        {
            public bool TryReceive(out T message)
            {
                message = default(T);
                return true;
            }
        }

        private AutoMoqer _mocker;
        private Mock<ILog> _log;
        private Func<Type, object> _serviceLocator;

        [SetUp]
        public void SetUp()
        {
            var testContainer = new UnityContainer()
                .RegisterType(typeof (IMessageHandler<>), typeof (FakeHandler<>))
                .RegisterType(typeof (IMessageQueueInbound<>), typeof (FakeQueue<>));

            _mocker = new AutoMoqer(testContainer);
            _log = _mocker.GetMock<ILog>();

            _serviceLocator = type => _mocker.Create(type);
        }


        public struct FakeAnalyticsMessage : IMessage { }
        public struct FakeAnalyticsMessage2 : IMessage { }

        [Test]
        public void Manafacture_CanManufactureCollection()
        {
            var factory = new MessageProcessorCollectionFactory(_log.Object);
            var processors = factory.Manafacture(_serviceLocator);

            Assert.That(processors, Has.Some.AssignableFrom(typeof (MessageProcessor<FakeAnalyticsMessage>)));
            Assert.That(processors, Has.Some.AssignableFrom(typeof (MessageProcessor<FakeAnalyticsMessage2>)));
        }
    }
}
