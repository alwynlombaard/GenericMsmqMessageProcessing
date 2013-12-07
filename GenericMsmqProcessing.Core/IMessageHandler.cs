using System;

namespace GenericMsmqProcessing.Core
{
    public interface IMessageHandler<in T> : IDisposable
    {
        void HandleMessage(T message);
    }
}
