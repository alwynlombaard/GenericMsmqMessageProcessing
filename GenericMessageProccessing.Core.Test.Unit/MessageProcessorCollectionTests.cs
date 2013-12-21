using GenericMsmqProcessing.Core;
using GenericMsmqProcessing.Core.MessageProccessor;
using Moq;
using NUnit.Framework;

namespace GenericMessageProccessing.Core.Test.Unit
{
    [TestFixture]
    [Category("Fast")]
    public class MessageProcessorCollectionTests
    {
        private MessageProcessorCollection _processors;
        private Mock<IMessageProcessor<IMessage>> _processor1;
        private Mock<IMessageProcessor<IMessage>> _processor2;

        [SetUp]
        public void SetUp()
        {
            _processor1 = new Mock<IMessageProcessor<IMessage>>();
            _processor2 = new Mock<IMessageProcessor<IMessage>>();
            _processors = new MessageProcessorCollection
            {
                _processor1.Object,
                _processor2.Object
            };
        }

        [Test]
        public void TotalNumberOfMessagesPickedUp()
        {
            _processor1.SetupGet(p => p.NumberOfMessagesPickedUp).Returns(3);
            _processor2.SetupGet(p => p.NumberOfMessagesPickedUp).Returns(4);

            Assert.That(_processors.TotalNumberOfMessagesPickedUp, Is.EqualTo(7));
        }

        [Test]
        public void TotalNumberOfMessageErrors()
        {
            _processor1.SetupGet(p => p.NumberOfMessageErrors).Returns(5);
            _processor2.SetupGet(p => p.NumberOfMessageErrors).Returns(6);

            Assert.That(_processors.TotalNumberOfMessageErrors, Is.EqualTo(11));
        }
    }

}
