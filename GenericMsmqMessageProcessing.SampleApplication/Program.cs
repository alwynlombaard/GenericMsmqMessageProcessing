using System;
using GenericMsmqProcessing.Core;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.MessageProccessor;
using GenericMsmqProcessing.Core.Queue.Msmq;

namespace GenericMsmqMessageProcessing.SampleApplication
{
    [Serializable]
    public struct MyMessage : IMessage
    {
        public int Id { get; set; }
    }

    public class MyMessageHandler : IMessageHandler<MyMessage>
    {
        public void HandleMessage(MyMessage message)
        {
            //handle your message here
            Console.WriteLine("HandleMessage " + message.Id);
            if (message.Id % 10 == 0)
            {
                throw new Exception("Bad things happened when handling message " + message.Id);
            }
        }

        public void OnError(MyMessage message, Exception ex)
        {
            //handle your errors here
            Console.WriteLine(ex.Message);
        }

        public void Dispose()
        {
            //this is invoked after the message has been handled
        }
    }

    public class Program
    {

        static void Main()
        {
            var messageProcessorCollection = MessageProcessorCollectionFactory.Collection();
            messageProcessorCollection.StartAll();

            for (var i = 1; i <= 100; i++)
            {
                try
                {
                    var queue = new MsmqMessageQueueOutbound<MyMessage>();
                    var message = new MyMessage { Id = i };
                    queue.Send(message);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error sending message" + ex.StackTrace);
                }
            }

            Console.ReadLine();
            messageProcessorCollection.StopAll();

        }
    }
}
