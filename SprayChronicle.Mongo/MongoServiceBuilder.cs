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
            _services.AddSingleton(s => new MongoProjector<TProjector>(
                s.GetRequiredService<ILoggerFactory>().CreateLogger<TProjector>(),
                s.GetRequiredService<IStoreEvents>(),
                s.GetRequiredService<IMongoDatabase>(),
                s.GetRequiredService<TProjector>(),
                batchSize,
                timeout
            ));
            
            if (_hostedServices) {
                _services.AddSingleton<IHostedService, BackgroundService<MongoProjector<TProjector>>>(s => new BackgroundService<MongoProjector<TProjector>>(
                    s.GetRequiredService<ILoggerFactory>().CreateLogger<MongoProjector<TProjector>>(),
                    s.GetRequiredService<MongoProjector<TProjector>>()
                ));
            }
            
            return this;
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
            _services.AddSingleton(s => new Processor<TProcess>(
                s.GetRequiredService<ILoggerFactory>().CreateLogger<TProcess>(),
                s.GetRequiredService<IStoreEvents>(),
                s.GetRequiredService<TProcess>()
            ));
            
            if (_hostedServices) {
                _services.AddSingleton<IHostedService, BackgroundService<Processor<TProcess>>>(s => new BackgroundService<Processor<TProcess>>(
                    s.GetRequiredService<ILoggerFactory>().CreateLogger<Processor<TProcess>>(),
                    s.GetRequiredService<Processor<TProcess>>()
                ));
            }
            
            return this;
        }
    }
}
