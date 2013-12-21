using System;
using System.Collections.Generic;
using System.Linq;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.Queue;
using GenericMsmqProcessing.Core.Queue.Msmq;
using log4net;

namespace GenericMsmqProcessing.Core.MessageProccessor
{
    public static class MessageProcessorCollectionFactory
    {
        private static volatile IMessageProcessorCollection _collection;
        private static readonly object syncRoot = new Object();
        private static readonly ILog logger = LogManager.GetLogger("GenericMsmqProcessing");

        public static IMessageProcessorCollection Collection(Func<Type, object> serviceLocator)
        {
            if (_collection != null) { return _collection; }
            lock (syncRoot)
            {
                if (_collection == null)
                {
                    _collection = FromServiceLocator(serviceLocator);
                }
            }
            return _collection;
        }

        public static IMessageProcessorCollection Collection()
        {
            if (_collection != null) { return _collection; }
            lock (syncRoot)
            {
                if (_collection == null)
                {
                    _collection = FromActivator();
                }
            }
            return _collection;
        }

        private static IMessageProcessorCollection FromServiceLocator(Func<Type, object> serviceLocator)
        {
            var processors = new MessageProcessorCollection();

            var messageTypes = GetMessageTypes();
            foreach (var messageType in messageTypes)
            {
                var queueArgumentType = GetGenericType(typeof(IMessageQueueInbound<>), messageType);
                var queueArgument = serviceLocator(queueArgumentType);

                var handlersArgumentType = typeof(List<>).MakeGenericType(typeof(IMessageHandler<>).MakeGenericType(messageType));
                var handlersArgument = Activator.CreateInstance(handlersArgumentType);

                var handlerImplementationTypes = GetMessageHandlerTypes(messageType).ToList();
                foreach (var handlerImplementationType in handlerImplementationTypes)
                {
                    var handler = serviceLocator(handlerImplementationType);
                    handlersArgumentType.GetMethod("Add").Invoke(handlersArgument, new[] { handler });
                }

                if (!handlerImplementationTypes.Any())
                {
                    logger.WarnFormat("No handlers found for {0}, processor not created ", messageType.Name);
                    continue;
                }

                var processorType = GetGenericType(typeof(MessageProcessor<>), messageType);
                var processor =
                    (IMessageProcessor)Activator.CreateInstance(processorType, queueArgument, handlersArgument);

                processors.Add(processor);
                logger.DebugFormat("Created processor for {0} with {1} handler(s)", messageType.Name, handlerImplementationTypes.Count());
            }

            return processors;
        }

        private static IMessageProcessorCollection FromActivator()
        {
            var processors = new MessageProcessorCollection();

            var messageTypes = GetMessageTypes();
            foreach (var messageType in messageTypes)
            {
                var queueArgumentType = GetGenericType(typeof(MsmqMessageQueueInbound<>), messageType);
                var queueArgument = Activator.CreateInstance(queueArgumentType);

                var handlersArgumentType = typeof(List<>).MakeGenericType(typeof(IMessageHandler<>).MakeGenericType(messageType));
                var handlersArgument = Activator.CreateInstance(handlersArgumentType);

                var handlerImplementationTypes = GetMessageHandlerTypes(messageType);
                foreach (var handlerImplementationType in handlerImplementationTypes)
                {
                    var handler = Activator.CreateInstance(handlerImplementationType);
                    handlersArgumentType.GetMethod("Add").Invoke(handlersArgument, new[] { handler });
                }

                var processorType = GetGenericType(typeof(MessageProcessor<>), messageType);
                var processor = (IMessageProcessor)Activator.CreateInstance(processorType, queueArgument, handlersArgument);

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

        private static IEnumerable<Type> GetMessageHandlerTypes(Type messageType)
        {
            var messageHandlerType = GetGenericType(typeof(IMessageHandler<>), messageType);
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