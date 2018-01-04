using System;
using Autofac;
using System.Linq;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;

namespace SprayChronicle.QueryHandling
{
    public class QueryHandlingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<SubscriptionQueryProcessor>()
                .OnActivating(e => RegisterQueryExecutors(e.Context, e.Instance as SubscriptionQueryProcessor))
                .AsSelf()
                .As<IProcessQueries>()
                .SingleInstance();
            
            builder
                .Register<LoggingQueryProcessor>(c => new LoggingQueryProcessor(
                    c.Resolve<ILoggerFactory>().CreateLogger<IProcessQueries>(),
                    c.Resolve<SubscriptionQueryProcessor>()
                ))
                .SingleInstance();
        }

        private static void RegisterQueryExecutors(IComponentContext context, SubscriptionQueryProcessor processor)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IExecuteQueries>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IExecuteQueries)
                .ToList()
                .ForEach(h => processor.AddExecutors(h));
        }
        
        public sealed class QueryHandler<TState,THandler> : Autofac.Module
            where TState : class
            where THandler : QueryHandler<TState>
        {
            private readonly string _reference;

            private readonly string _stream;

            public QueryHandler(string stream)
                : this(typeof(TState).Name, stream)
            {}

            public QueryHandler(string reference, string stream)
            {
                _reference = reference;
                _stream = stream;
            }

            protected override void Load(ContainerBuilder builder)
            {
                builder
                    .Register(c => c.Resolve<IBuildStatefulRepositories>().Build<TState>(_reference))
                    .As<IStatefulRepository<TState>>()
                    .SingleInstance();
                
                builder
                    .Register(c => new StreamHandler<THandler>(
                        c.Resolve<ILoggerFactory>().CreateLogger<THandler>(),
                        c.Resolve<IBuildStreams>().CatchUp(_stream),
                        c.Resolve<THandler>()
                    ))
                    .As<IHandleStream>()
                    .AsSelf()
                    .OnlyIf(reg => reg.IsRegistered(new TypedService(typeof(IProcessQueries))))
                    .SingleInstance();
                
                builder
                    .RegisterType<THandler>()
                    .As<IExecuteQueries>()
                    .As<IHandleEvents>()
                    .AsSelf()
                    .SingleInstance();
            }
        }
    }
}