using System;
using Autofac;
using System.Linq;
using Autofac.Core;
using SprayChronicle.EventHandling;
using SprayChronicle.Server;

namespace SprayChronicle.QueryHandling
{
    public class QueryHandlingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<SubscriptionProcessor>()
                .OnActivating(e => RegisterQueryExecutors(e.Context, e.Instance))
                .AsSelf()
                .As<IProcessQueries>()
                .SingleInstance();
            
            builder
                .Register(c => new LoggingProcessor(
                    c.Resolve<ILoggerFactory>().Create<IProcessQueries>(),
                    new MeasureMilliseconds(),
                    c.Resolve<SubscriptionProcessor>()
                ))
                .SingleInstance();
        }

        private static void RegisterQueryExecutors(IComponentContext context, SubscriptionProcessor processor)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IExecuteQueries>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IExecuteQueries)
                .ToList()
                .ForEach(h => processor.Subscribe(h));
        }
        
        public sealed class QueryHandler<TState,THandler> : Module
            where TState : class
            where THandler : QueryHandler<THandler>
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
                
//                builder
//                    .Register(c => new StreamHandler<THandler>(
//                        c.Resolve<ILoggerFactory>().Create<THandler>(),
//                        new MeasureMilliseconds(),
//                        c.Resolve<IBuildStreams>().CatchUp(_stream),
//                        c.Resolve<THandler>()
//                    ))
//                    .As<IHandleStream>()
//                    .AsSelf()
//                    .OnlyIf(reg => reg.IsRegistered(new TypedService(typeof(IProcessQueries))))
//                    .SingleInstance();
                
                builder
                    .RegisterType<THandler>()
                    .As<IExecuteQueries>()
                    .As<IProcessEvents>()
                    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                    .AsSelf()
                    .SingleInstance();
            }
        }
    }
}