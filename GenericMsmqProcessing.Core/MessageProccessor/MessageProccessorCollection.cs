using System.Collections.Generic;

namespace GenericMsmqProcessing.Core.MessageProccessor
{
    public class MessageProccessorCollection : List<IMessageProcessor>,  IMessageProccessorCollection
    {
        public void StartAll()
        {
            foreach (var processor in this)
            {
                processor.Start();
            }
        }

        public void StopAll()
        {
            foreach (var processor in this)
            {
                processor.Stop();
            }
        }
    }
}