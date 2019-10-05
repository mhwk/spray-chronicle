using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace SprayChronicle.Mongo
{
    public class MongoServiceBuilder : IEventSourcingBuilder
    {
        private readonly IServiceCollection _services;
        private bool _hostedServices = true;

        public MongoServiceBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public MongoServiceBuilder DisableHostedServices()
        {
            _hostedServices = false;
            return this;
        }

        public IEventSourcingBuilder MapState(Action<IMapState> map)
        {
            _services.AddTransient(s => map);
            return this;
        }

        public IEventSourcingBuilder AddInvariant<TInvariant>()
            where TInvariant : class, IArrange<TInvariant>, IAct<TInvariant>
        {
            MapState(map => map.Map<TInvariant>(s => s.GetRequiredService<TInvariant>()));
            _services.AddTransient<TInvariant>();
            return this;
        }

        public IEventSourcingBuilder AddProjector<TProjector>(int batchSize, TimeSpan timeout)
            where TProjector : class, IProject
        {
            _services.AddSingleton<TProjector>();
            if (_hostedServices) {
                _services.AddSingleton<IHostedService, MongoProjector<TProjector>>(s => CreateProjector<TProjector>(s, batchSize, timeout));
            } else {
                _services.AddSingleton(s => CreateProjector<TProjector>(s, batchSize, timeout));
            }
            return this;
        }

        private static MongoProjector<TProjector> CreateProjector<TProjector>(IServiceProvider services, int batchSize, TimeSpan timeout)
            where TProjector : class, IProject
        {
            return new MongoProjector<TProjector>(
                services.GetRequiredService<ILoggerFactory>().CreateLogger<TProjector>(),
                services.GetRequiredService<IStoreEvents>(),
                services.GetRequiredService<IStoreSnapshots>(),
                services.GetRequiredService<IMongoDatabase>(),
                services.GetRequiredService<TProjector>(),
                batchSize,
                timeout
            );
        }

        public IEventSourcingBuilder AddProjector<TProjector>(TimeSpan timeout)
            where TProjector : class, IProject
        {
            AddProjector<TProjector>(100, timeout);
            return this;
        }

        public IEventSourcingBuilder AddProjector<TProjector>(int batchSize)
            where TProjector : class, IProject
        {
            AddProjector<TProjector>(batchSize, TimeSpan.FromMilliseconds(100));
            return this;
        }

        public IEventSourcingBuilder AddProcessor<TProcess>()
            where TProcess : class, IProcess
        {
            _services.AddSingleton<TProcess>();
            if (_hostedServices) {
                _services.AddSingleton<IHostedService, Processor<TProcess>>(CreateProcessor<TProcess>);
            } else {
                _services.AddSingleton(CreateProcessor<TProcess>);
            }
            return this;
        }

        private static Processor<TProcess> CreateProcessor<TProcess>(IServiceProvider services)
            where TProcess : class, IProcess
        {
            return new Processor<TProcess>(
                services.GetRequiredService<ILoggerFactory>().CreateLogger<TProcess>(),
                services.GetRequiredService<IStoreEvents>(),
                services.GetRequiredService<TProcess>()
            );
        }
    }
}
