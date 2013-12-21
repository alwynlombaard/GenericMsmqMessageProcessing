using System.Collections.Generic;
using System.Linq;
using System.Messaging;

namespace GenericMsmqProcessing.Core.MessageProccessor
{
    public class MessageProcessorCollection : List<IMessageProcessor>, IMessageProcessorCollection
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

        public int TotalNumberOfMessagesPickedUp
        {
            get
            {
                return this.Sum(item => item.NumberOfMessagesPickedUp);
            }
        }

        public int TotalNumberOfMessageErrors
        {
            get
            {
                return this.Sum(item => item.NumberOfMessageErrors);
            }
        }

        public IEnumerable<MessageQueue> QueuesForProcessors()
        {
            var queues = from queue in MessageQueue.GetPrivateQueuesByMachine(".")
                         from processor in this
                         where queue.QueueName.ToLowerInvariant().Contains(processor.Name.ToLowerInvariant())
                         select queue;
            return queues;
        }
    }
}
