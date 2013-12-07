using System.Collections.Generic;

namespace GenericMsmqProcessing.Core.MessageProccessor
{
    public interface IMessageProccessorCollection : IList<IMessageProcessor>
    {
        void StartAll();
        void StopAll();
    }
}