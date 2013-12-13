namespace GenericMsmqProcessing.Core.MessageProccessor
{
    public interface IMessageProcessorCollectionFactory
    {
        IMessageProccessorCollection Manufacture();
    }
}
