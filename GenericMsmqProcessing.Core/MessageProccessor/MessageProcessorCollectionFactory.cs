using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GenericMsmqProcessing.Core.MessageHandler;
using GenericMsmqProcessing.Core.Queue;
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

        public IMessageProccessorCollection Manafacture(Func<Type, object> serviceLocator)
        {
            var messageTypes = GetMessageTypes();

            var processors = new MessageProccessorCollection();
            foreach (var messageType in messageTypes)
            {
                var queueArgumentType = GetGenericType(typeof(IMessageQueueInbound<>), messageType);
                var queueArgument = serviceLocator(queueArgumentType);

                var handlersArgumentType = typeof(List<>).MakeGenericType(typeof(IMessageHandler<>).MakeGenericType(messageType));
                var handlersArgument = Activator.CreateInstance(handlersArgumentType);
     
                var handlerImplementationTypes = GetMessageHandlerTypes(messageType);
                foreach (var handlerImplementationType in handlerImplementationTypes)
                {
                    var handler = serviceLocator(handlerImplementationType);
                    handlersArgumentType.GetMethod("Add").Invoke(handlersArgument, new[] { handler });
                }

                var processorType = GetGenericType(typeof(MessageProcessor<>), messageType);

                var manufactureMethod = processorType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == "Manufacture")
                    .ToList().FirstOrDefault(m => m.GetParameters().ToList().Exists((p => p.Name == "messageHandlers")));

                if (manufactureMethod == null) continue;
                var processor = (IMessageProcessor)manufactureMethod.Invoke(null, new[] {queueArgument, handlersArgument});

                processors.Add(processor);
                _log.InfoFormat("Added {0} Processor", processor.Name);
            }

            return processors;
        }

        public IMessageProccessorCollection Manufacture()
        {
            var processors = new MessageProccessorCollection();

            var messageTypes = GetMessageTypes();
            foreach (var messageType in messageTypes)
            {
               //queue 
                var queueArgumentType = GetGenericType(typeof (MsmqMessageQueueInbound<>), messageType);
                var queueArgument = Activator.CreateInstance(queueArgumentType, _log);

                //handlers
                var handlersArgumentType = typeof(List<>).MakeGenericType(typeof (IMessageHandler<>).MakeGenericType(messageType));
                var handlersArgument = Activator.CreateInstance(handlersArgumentType);

                var handlerImplementationTypes = GetMessageHandlerTypes(messageType);
                foreach (var handlerImplementationType in handlerImplementationTypes)
                {
                    var handler = Activator.CreateInstance(handlerImplementationType);
                    handlersArgumentType.GetMethod("Add").Invoke(handlersArgument, new[] { handler });
                }
                
                var processorType = GetGenericType(typeof(MessageProcessor<>), messageType);
                
                var manufactureMethod = processorType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == "Manufacture")
                    .ToList().FirstOrDefault(m => m.GetParameters().ToList().Exists((p => p.Name == "messageHandlers")));

                if (manufactureMethod == null) continue;
          
                var processor = (IMessageProcessor)manufactureMethod.Invoke(null, new[] { queueArgument, handlersArgument });

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