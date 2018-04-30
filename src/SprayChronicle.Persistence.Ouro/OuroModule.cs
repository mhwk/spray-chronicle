using System;
using System.Net;
using System.Threading.Tasks;
using App.Metrics.Health;
using Autofac;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Projections;
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
                    c.Resolve<ILoggerFactory>(),
                    c.Resolve<IEventStoreConnection>(),
                    c.Resolve<ProjectionsManager>(), 
                    c.Resolve<UserCredentials>()
                ))
                .AsSelf()
                .As<IEventStore>()
                .As<IEventSourceFactory>()
                .SingleInstance();

            builder
                .Register(c => new ProjectionsManager(
                    c.Resolve<OuroLogger>(),
                    new DnsEndPoint(
                        ChronicleServer.Env(
                            "EVENTSTORE_HOST",
                            ChronicleServer.Env(
                                "EVENTSTORE_CLUSTER_DNS"
                            )
                        ),
                        2113
                    ),
                    TimeSpan.FromSeconds(5)
                ))
                .AsSelf()
                .SingleInstance();

            builder
                .Register(c => new OuroLogger(c.Resolve<ILoggerFactory>().Create<OuroEventStore>()))
                .SingleInstance();
            
            builder
                .Register(c => new OuroHealthCheck(c.Resolve<IEventStoreConnection>()))
                .AsSelf()
                .As<HealthCheck>()
                .SingleInstance();
        }

        private static IEventStoreConnection InitEventStore(IComponentContext container)
        {
            return "" != (ChronicleServer.Env("EVENTSTORE_CLUSTER_DNS", ""))
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
                    .UseCustomLogger(container.Resolve<OuroLogger>())
                    .Build(),
				new Uri (uri)
			);
		    
			connection.ConnectAsync().Wait();
		    
			return connection;
		}

        private static IEventStoreConnection InitEventStoreCluster(IComponentContext container)
        {
			var connection = EventStoreConnection.Create (
                ConnectionSettings.Create()
                    .WithConnectionTimeoutOf(TimeSpan.FromSeconds(5))
                    .KeepReconnecting()
                    .KeepRetrying()
                    .PerformOnAnyNode()
                    .UseCustomLogger(container.Resolve<OuroLogger>())
                    .SetDefaultUserCredentials(new UserCredentials(
                        ChronicleServer.Env("EVENTSTORE_USERNAME", "admin"),
                        ChronicleServer.Env("EVENTSTORE_PASSWORD", "changeit")
                    )),
                ClusterSettings.Create().DiscoverClusterViaDns()
                    .SetClusterDns(ChronicleServer.Env("EVENTSTORE_CLUSTER_DNS", "eventstore"))
                    .SetClusterGossipPort(Int32.Parse(ChronicleServer.Env("EVENTSTORE_GOSSIP_PORT", "2113")))
                    .PreferRandomNode()
			);
            
			connection.ConnectAsync().Wait();

			return connection;
        }
    }
}
