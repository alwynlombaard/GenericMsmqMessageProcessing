using System;
using GenericMsmqProcessing.Core;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.MessageProccessor;
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
        [SetUp]
        public void SetUp()
        {
           
        }

        [Test]
        public void ManufactureCanManufactureMessageProcessors()
        {
            var processors = MessageProcessorCollectionFactory.Collection();

            Assert.That(processors, Has.Some.AssignableFrom(typeof(MessageProcessor<MyFakeMessage>)));
        }
    }
}
