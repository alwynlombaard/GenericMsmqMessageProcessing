namespace GenericMsmqProcessing.Core.Queue
{
    public interface IMessageQueueInbound<T>
    {
        bool TryReceive(out T message);
    }
}
