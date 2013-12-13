using GenericMsmqProcessing.Core;

namespace GenericMsmqMessageProcessing.Test.Integration
{
    public interface IDoSomeWorkWithIMessage
    {
        void DoWork(IMessage message);
    }
}