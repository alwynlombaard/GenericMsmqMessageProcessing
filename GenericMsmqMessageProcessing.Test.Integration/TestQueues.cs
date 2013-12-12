using System;
using System.Linq;
using System.Messaging;

namespace GenericMsmqMessageProcessing.Test.Integration
{
    public static class TestQueues
    {
        public static void PurgeQueues()
        {
            try
            {
                var queue = MessageQueue.GetPrivateQueuesByMachine(".")
                    .FirstOrDefault(
                        q => q.QueueName.ToLowerInvariant().Contains(typeof(MyMessage).Name.ToLowerInvariant()));

                if (queue != null)
                {
                    queue.Purge();
                }

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Could not purge test queue", ex);
            }
        } 
    }
}