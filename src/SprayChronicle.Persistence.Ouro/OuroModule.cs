using System;
using Autofac;
using Microsoft.Extensions.Logging;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<IEventStoreConnection>(c => InitEventStore(c))
                .SingleInstance();
            
            builder
                .Register<UserCredentials>(c => new UserCredentials(
                    Environment.GetEnvironmentVariable("EVENTSTORE_USERNAME") ?? "admin",
                    Environment.GetEnvironmentVariable("EVENTSTORE_PASSWORD") ?? "changeit"
                ))
                .SingleInstance();
            
            builder
                .Register<IEventStore>(c => new OuroEventStore(
                    c.Resolve<ILoggerFactory>().CreateLogger<IEventStore>(),
                    c.Resolve<IEventStoreConnection>(),
                    c.Resolve<UserCredentials>(),
                    Environment.GetEnvironmentVariable("EVENTSTORE_TENANT")
                ))
                .AsSelf()
                .As<IEventStore>()
                .SingleInstance();
            
            builder
                .Register<OuroStreamFactory>(c => new OuroStreamFactory(
                    c.Resolve<ILoggerFactory>().CreateLogger<IEventStore>(),
                    c.Resolve<IEventStoreConnection>(),
                    c.Resolve<UserCredentials>(),
                    Environment.GetEnvironmentVariable("EVENTSTORE_TENANT") ?? "default"
                ))
                .AsSelf()
                .As<IBuildStreams>()
                .SingleInstance();
        }

        IEventStoreConnection InitEventStore(IComponentContext container)
        {
            if ("" != (Environment.GetEnvironmentVariable("EVENTSTORE_CLUSTER_DNS") ?? "")) {
                return InitEventStoreCluster(container);
            } else if ("" != (Environment.GetEnvironmentVariable("EVENTSTORE_HOST") ?? "")) {
                return InitEventStoreSingle(container);
            } else {
                throw new Exception("Please provide either EVENTSTORE_CLUSTER_DNS or EVENTSTORE_HOST environment vairables");
            }
        }

        IEventStoreConnection InitEventStoreSingle(IComponentContext container)
		{
            var uri = string.Format(
                "tcp://{0}:{1}@{2}:{3}",
                Environment.GetEnvironmentVariable("EVENTSTORE_USERNAME") ?? "admin",
                Environment.GetEnvironmentVariable("EVENTSTORE_PASSWORD") ?? "changeit",
                Environment.GetEnvironmentVariable("EVENTSTORE_HOST") ?? "127.0.0.1",
                Environment.GetEnvironmentVariable("EVENTSTORE_PORT") ?? "1113"
            );

            container.Resolve<ILoggerFactory>().CreateLogger<IEventStoreConnection>().LogInformation("Connecting to eventstore on {0}", uri);
            
			IEventStoreConnection connection = EventStoreConnection.Create (
				ConnectionSettings.Create()
                    .WithConnectionTimeoutOf(TimeSpan.FromSeconds(5))
                    .KeepReconnecting()
                    .KeepRetrying()
                    .UseConsoleLogger()
                    .Build(),
				new Uri (uri)
			);
			connection.ConnectAsync().Wait();
			return connection;
		}

        IEventStoreConnection InitEventStoreCluster(IComponentContext container)
        {
            var logger = container.Resolve<ILoggerFactory>().CreateLogger<IEventStoreConnection>();

            logger.LogInformation(
                "Connecting to eventstore cluster dns {0}:{1}",
                Environment.GetEnvironmentVariable("EVENTSTORE_CLUSTER_DNS"),
                Environment.GetEnvironmentVariable("EVENTSTORE_GOSSIP_PORT") ?? "2113"
            );
            
			IEventStoreConnection connection = EventStoreConnection.Create (
                ConnectionSettings.Create()
                    .KeepReconnecting()
                    .PerformOnAnyNode()
                    .SetDefaultUserCredentials(new UserCredentials(
                        Environment.GetEnvironmentVariable("EVENTSTORE_USERNAME") ?? "admin",
                        Environment.GetEnvironmentVariable("EVENTSTORE_PASSWORD") ?? "changeit"
                    )),
                ClusterSettings.Create().DiscoverClusterViaDns()
                    .SetClusterDns(Environment.GetEnvironmentVariable("EVENTSTORE_CLUSTER_DNS") ?? "eventstore")
                    .SetClusterGossipPort(Int32.Parse(Environment.GetEnvironmentVariable("EVENTSTORE_GOSSIP_PORT") ?? "2113"))
                    .PreferRandomNode()
			);
			connection.ConnectAsync().Wait();
			return connection;
        }
    }
}
