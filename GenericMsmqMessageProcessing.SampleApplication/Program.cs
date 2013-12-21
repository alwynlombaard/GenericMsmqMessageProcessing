using System;
using GenericMsmqProcessing.Core;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.MessageProccessor;
using GenericMsmqProcessing.Core.Queue.Msmq;
using log4net;

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
            Program.Logger.InfoFormat("HandleMessage: handling message {0}", message.Id);
            if (message.Id % 3 == 0)
            {
                throw new Exception("Simulated exception for message  " + message.Id);
            }
        }

        public void OnError(MyMessage message, Exception ex)
        {
            //handle your errors here
            Program.Logger.Warn("OnError: Handled exception for message " + message.Id,   ex);
        }

        public void Dispose()
        {
            //this is invoked after the message has been handled
        }
    }

    public class Program
    {
        public static readonly ILog Logger = LogManager.GetLogger("GenericMsmqMessageProcessing.SampleApplication");

        static Program()
        {
            LoggingConfiguration.Configure();
        }

        static void Main()
        {
            Logger.InfoFormat("Queueing test messages...{0}", Environment.NewLine);
            QueueMessages();

            Logger.InfoFormat("Starting message processors...{0}", Environment.NewLine);
            var messageProcessorCollection = MessageProcessorCollectionFactory.Collection();
            messageProcessorCollection.StartAll();
            
            Console.ReadKey();
            messageProcessorCollection.StopAll();
        }

        private static void QueueMessages()
        {
            for (var i = 1; i <= 4; i++)
            {
                try
                {
                    Logger.InfoFormat("Queueing message {0}", i);
                    var queue = new MsmqMessageQueueOutbound<MyMessage>();
                    var message = new MyMessage {Id = i};
                    queue.Send(message);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error queueing message " + i, ex);
                }
            }
        }

        static class LoggingConfiguration
        {
            public static void Configure()
            {
                log4net.Config.XmlConfigurator.Configure();
            }
        }
    }
}
