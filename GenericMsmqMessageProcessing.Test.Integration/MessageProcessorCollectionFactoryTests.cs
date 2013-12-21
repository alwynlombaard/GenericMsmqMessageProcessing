using System;
using AutoMoq;
using GenericMsmqProcessing.Core;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.MessageProccessor;
using log4net;
using NUnit.Framework;

namespace GenericMsmqMessageProcessing.Test.Integration
{
    public struct MyFakeMessage : IMessage { }

    public class MyFakeMessageHandler : IMessageHandler<MyFakeMessage>
    {
        public void HandleMessage(MyFakeMessage message) { }
        public void OnError(MyFakeMessage message, Exception ex) { }
        public void Dispose() { }
    }

    [TestFixture]
    [Category("Slow")]
    public class MessageProcessorCollectionFactoryTests
    {
        private AutoMoqer _mocker;
        private ILog _log;

        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMoqer();
            _log = _mocker.GetMock<ILog>().Object;
        }



        [Test]
        public void ManufactureCanManufactureMessageProcessors()
        {
            var processors = MessageProcessorCollectionFactory.Collection(_log);

            Assert.That(processors, Has.Some.AssignableFrom(typeof(MessageProcessor<MyFakeMessage>)));
        }
    }
}
