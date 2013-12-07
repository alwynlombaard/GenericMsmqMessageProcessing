namespace GenericMsmqProcessing.Core
{
    public interface IMessageQueueInbound<T>
    {
        bool TryReceive(out T message);
    }
}
