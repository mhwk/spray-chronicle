using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.CommandHandling
{
    public class CommandHandlingModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<SubscriptionCommandBus>(c => new SubscriptionCommandBus())
                .OnActivating(e => RegisterCommandHandlers(e.Context, e.Instance as SubscriptionCommandBus))
                .SingleInstance();
            
            builder
                .Register<LoggingCommandBus>(c => new LoggingCommandBus(
                    c.Resolve<ILoggerFactory>().CreateLogger<LoggingCommandBus>(),
                    c.Resolve<SubscriptionCommandBus>()
                ))
                .SingleInstance();
            
            builder
                .Register<ThreadedCommandBus>(c => new ThreadedCommandBus(
                    c.Resolve<LoggingCommandBus>()
                ))
                .SingleInstance();
        }

        void RegisterCommandHandlers(IComponentContext context, SubscriptionCommandBus dispatcher)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IHandleCommand>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IHandleCommand)
                .ToList()
                .ForEach(h => dispatcher.Subscribe(h));
        }

        public class OverloadHandler<THandler,TSourced> : Autofac.Module where THandler : OverloadCommandHandler<TSourced> where TSourced : EventSourced<TSourced>
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder
                    .Register<IEventSourcingRepository<TSourced>>(c => new EventSourcedRepository<TSourced>(
                            c.Resolve<IEventStore>()
                    ))
                    .SingleInstance();
                
                builder
                    .Register<THandler>(c => Activator.CreateInstance(
                        typeof(THandler),
                        BuildArguments(c)
                    ) as THandler)
                    .As<IHandleCommand>()
                    .AsSelf()
                    .InstancePerDependency();
            }

            object[] BuildArguments(IComponentContext context)
            {
                var args = new List<object>();

                var constructor = typeof(THandler).GetTypeInfo().GetConstructors()
                    .OrderByDescending(c => c.GetParameters().Length)
                    .FirstOrDefault();
                
                if (null == constructor) {
                    return args.ToArray();
                }

                var types = constructor.GetParameters()
                    .Select(p => p.ParameterType)
                    .ToArray();
                
                for (var i = 0; i < types.Length; i++) {
                    args.Add(context.Resolve(types[i]));
                }

                return args.ToArray();
            }
        }
    }
}
