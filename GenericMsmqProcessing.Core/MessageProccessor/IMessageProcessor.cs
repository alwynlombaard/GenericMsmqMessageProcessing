namespace GenericMsmqProcessing.Core.MessageProccessor
{
    public interface IMessageProcessor<in T> : IMessageProcessor
    {
        
    }

    public interface IMessageProcessor 
    {
        void Start();
        void Stop();
        string Name { get; set; }
    }

    
}