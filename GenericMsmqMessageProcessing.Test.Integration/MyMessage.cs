using GenericMsmqProcessing.Core;

namespace GenericMsmqMessageProcessing.Test.Integration
{
    public partial class MyMessage : IMessage
    {
        public string Message { get; set; }
    }
}