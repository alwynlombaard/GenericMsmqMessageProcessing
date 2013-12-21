using System.Collections.Generic;
using System.Messaging;

namespace GenericMsmqProcessing.Core.MessageProccessor
{
    public interface IMessageProcessorCollection : IList<IMessageProcessor>
    {
        void StartAll();
        void StopAll();
        int TotalNumberOfMessagesPickedUp { get; }
        int TotalNumberOfMessageErrors { get; }
        IEnumerable<MessageQueue> QueuesForProcessors();
    }
}