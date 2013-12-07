namespace GenericMsmqProcessing.Core
{
    public interface IMessageQueueOutbound<in T>
    {
        void Send(T message);
    }
}
