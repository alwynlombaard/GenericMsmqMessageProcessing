namespace GenericMsmqProcessing.Core.Queue
{
    public interface IMessageQueueOutbound<in T>
    {
        void Send(T message);
    }
}
