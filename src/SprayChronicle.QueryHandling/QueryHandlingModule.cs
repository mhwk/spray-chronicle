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
                .RegisterType<SubscriptionRouter>()
                .OnActivating(e => RegisterQueryExecutors(e.Context, e.Instance))
                .AsSelf()
                .As<IQueryRouter>()
                .SingleInstance();
            
            builder
                .Register(c => new LoggingRouter(
                    c.Resolve<ILoggerFactory>().Create<IQueryRouter>(),
                    new MeasureMilliseconds(),
                    c.Resolve<SubscriptionRouter>()
                ))
                .SingleInstance();
        }

        private static void RegisterQueryExecutors(IComponentContext context, SubscriptionRouter router)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IQueryExecutor>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IQueryExecutor)
                .ToList()
                .ForEach(h => router.Subscribe(h));
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
                    .As<IQueryExecutor>()
                    .As<IProcessEvents>()
                    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                    .AsSelf()
                    .SingleInstance();
            }
        }
    }
}