using System;
using System.Collections.Generic;
using System.Linq;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.Queue.Msmq;
using log4net;

namespace GenericMsmqProcessing.Core.MessageProccessor
{
    public class MessageProcessorCollectionFactory : IMessageProcessorCollectionFactory
    {
        private readonly ILog _log;

        public MessageProcessorCollectionFactory(ILog log)
        {
            _log = log;
        }

        public IMessageProccessorCollection Manufacture()
        {
            var processors = new MessageProccessorCollection();

            var messageTypes = GetMessageTypes();
            foreach (var messageType in messageTypes)
            {
                var handlerTypes = GetMessageHandlers(messageType);

                //there can be only one processor per messageType
                var handlerType = handlerTypes.FirstOrDefault();
                if (handlerType == null) continue;
                var queueType = GetGenericType(typeof (MsmqMessageQueueInbound<>), messageType);
                var processorType = GetGenericType(typeof (MessageProcessor<>), messageType);

                var queue = Activator.CreateInstance(queueType, _log);
                var handler = Activator.CreateInstance(handlerType);
                var processor = (IMessageProcessor) Activator.CreateInstance(processorType, queue, handler);

                processors.Add(processor);
            }
            return processors;
        }

        private static IEnumerable<Type> GetMessageTypes()
        {
            return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from type in assembly.GetTypes()
                    let interfaces = type.GetInterfaces()
                    where interfaces.Any(x => x == typeof(IMessage))
                    select type).AsQueryable();
        }

        private static IEnumerable<Type> GetMessageHandlers(Type messageType)
        {
            var messageHandlerType = GetGenericType(typeof (IMessageHandler<>), messageType);
            return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from type in assembly.GetTypes()
                    let interfaces = type.GetInterfaces()
                    where interfaces.Any(x => x == messageHandlerType)
                    select type).AsQueryable();
        }

        private static Type GetGenericType(Type mainType, params Type[] genericTypesArgs)
        {
            return mainType.MakeGenericType(genericTypesArgs);
        }
    }
}