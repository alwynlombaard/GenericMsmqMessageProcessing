namespace GenericMsmqMessageProcessing.Test.Integration
{
    public interface IDoSomeWorkWithMyMessage
    {
        void DoWork(MyMessage message);
    }
}