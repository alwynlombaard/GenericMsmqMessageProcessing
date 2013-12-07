using System.Collections.Generic;

namespace GenericMsmqProcessing.Core
{
    public interface IMessageProccessorCollection : IList<IMessageProcessor>
    {
        void StartAll();
        void StopAll();
    }
}