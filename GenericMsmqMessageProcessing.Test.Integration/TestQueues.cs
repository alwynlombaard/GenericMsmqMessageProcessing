using System;
using System.Linq;
using System.Messaging;

namespace GenericMsmqMessageProcessing.Test.Integration
{
    public static class TestQueues
    {
        public static void PurgeQueues()
        {
            var queues = MessageQueue.GetPrivateQueuesByMachine(".")
                .Where(
                    q => 
                        q.QueueName.ToLowerInvariant().Contains(typeof (MyMessageForMsmqMessageQueueTests).Name.ToLowerInvariant())
                        || q.QueueName.ToLowerInvariant().Contains(typeof (MyMessageForFactoryTesting).Name.ToLowerInvariant())
                         || q.QueueName.ToLowerInvariant().Contains(typeof (MyFakeMessage).Name.ToLowerInvariant()));

            foreach (var queue in queues)
            {
                try
                {
                    queue.Purge();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Could not purge test queue", ex);
                }
            }

        }

    }
}
