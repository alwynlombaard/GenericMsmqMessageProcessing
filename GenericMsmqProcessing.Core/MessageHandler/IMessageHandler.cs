using System;

namespace GenericMsmqProcessing.Core.MessageHandler
{
    public interface IMessageHandler<in T> : IDisposable
    {
        void HandleMessage(T message);
        void OnError(T message, Exception ex);
    }
}
