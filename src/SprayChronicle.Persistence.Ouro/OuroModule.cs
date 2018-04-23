using System;
using App.Metrics.Health;
using Autofac;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SprayChronicle.EventSourcing;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(InitEventStore)
                .As<IEventStoreConnection>()
                .SingleInstance();
            
            builder
                .Register(c => new UserCredentials(
                    ChronicleServer.Env("EVENTSTORE_USERNAME", "admin"),
                    ChronicleServer.Env("EVENTSTORE_PASSWORD", "changeit")
                ))
                .SingleInstance();
            
            builder
                .Register(c => new OuroEventStore(
                    c.Resolve<ILoggerFactory>().Create<IEventStore>(),
                    c.Resolve<OuroSourceFactory>(),
                    c.Resolve<IEventStoreConnection>(),
                    c.Resolve<UserCredentials>()
                ))
                .AsSelf()
                .As<IEventStore>()
                .SingleInstance();
            
            builder
                .Register(c => new OuroSourceFactory(
                    c.Resolve<ILoggerFactory>(),
                    c.Resolve<IEventStoreConnection>(),
                    c.Resolve<UserCredentials>()
                ))
                .AsSelf()
                .As<IEventSourceFactory>()
                .SingleInstance();
            
            builder
                .Register(c => new OuroHealthCheck(c.Resolve<IEventStoreConnection>()))
                .AsSelf()
                .As<HealthCheck>()
                .SingleInstance();
        }

        private static IEventStoreConnection InitEventStore(IComponentContext container)
        {
            return "" != (ChronicleServer.Env("EVENTSTORE_CLUSTER_DNS") ?? "")
                ? InitEventStoreCluster(container)
                : InitEventStoreSingle(container);
        }

        private static IEventStoreConnection InitEventStoreSingle(IComponentContext container)
		{
            var uri = string.Format(
                "tcp://{0}:{1}@{2}:{3}",
                ChronicleServer.Env("EVENTSTORE_USERNAME", "admin"),
                ChronicleServer.Env("EVENTSTORE_PASSWORD", "changeit"),
                ChronicleServer.Env("EVENTSTORE_HOST", "127.0.0.1"),
                ChronicleServer.Env("EVENTSTORE_PORT", "1113")
            );
			var connection = EventStoreConnection.Create (
				ConnectionSettings.Create()
                    .WithConnectionTimeoutOf(TimeSpan.FromSeconds(5))
                    .KeepReconnecting()
                    .KeepRetrying()
                    .UseConsoleLogger()
                    .Build(),
				new Uri (uri)
			);
		    
			connection
			    .ConnectAsync()
			    .Wait();
		    
            container
                .Resolve<ILoggerFactory>()
                .Create<IEventStoreConnection>()
                .LogInformation($"Connected to eventstore on {uri}!");

			return connection;
		}

        private static IEventStoreConnection InitEventStoreCluster(IComponentContext container)
        {
            var logger = container.Resolve<ILoggerFactory>().Create<IEventStoreConnection>();
            
			var connection = EventStoreConnection.Create (
                ConnectionSettings.Create()
                    .KeepReconnecting()
                    .PerformOnAnyNode()
                    .SetDefaultUserCredentials(new UserCredentials(
                        ChronicleServer.Env("EVENTSTORE_USERNAME") ?? "admin",
                        ChronicleServer.Env("EVENTSTORE_PASSWORD") ?? "changeit"
                    )),
                ClusterSettings.Create().DiscoverClusterViaDns()
                    .SetClusterDns(ChronicleServer.Env("EVENTSTORE_CLUSTER_DNS") ?? "eventstore")
                    .SetClusterGossipPort(Int32.Parse(ChronicleServer.Env("EVENTSTORE_GOSSIP_PORT") ?? "2113"))
                    .PreferRandomNode()
			);
			connection.ConnectAsync().Wait();

            logger.LogInformation(
                $"Connected to eventstore cluster dns {ChronicleServer.Env("EVENTSTORE_CLUSTER_DNS")}:{ChronicleServer.Env("EVENTSTORE_GOSSIP_PORT")}!"
            );

			return connection;
        }
    }
}
