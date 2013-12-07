using AutoMoq;
using GenericMsmqProcessing.Core;
using Moq;
using NUnit.Framework;
using ReallySimpleEventing;

namespace GenericMessageProccessing.Core.Test.Unit
{
    public class FakeAnalyticsMessage : IMessage
    {

    }

    [TestFixture]
    [Category("Fast")]
    public class MessageHandlerTests
    {
        private AutoMoqer _mocker;
        private Mock<IEventStream> _eventStream;
        private FakeAnalyticsMessage _fakeMessage;

        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMoqer();
            _eventStream = _mocker.GetMock<IEventStream>();
            _fakeMessage = new FakeAnalyticsMessage();
        }

        [Test]
        public void HandleMessageRaisesEventForMessage()
        {
            var handler = _mocker.Resolve<MessageHandler<FakeAnalyticsMessage>>();

            handler.HandleMessage(_fakeMessage);

            _eventStream.Verify(x => x.Raise(_fakeMessage), Times.Once());

        }

    }
}
