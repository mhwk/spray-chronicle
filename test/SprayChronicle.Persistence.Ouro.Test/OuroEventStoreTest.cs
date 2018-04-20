using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using NSubstitute;
using Shouldly;
using SprayChronicle.EventSourcing;
using SprayChronicle.Server;
using Xunit;

namespace SprayChronicle.Persistence.Ouro.Test
{
    public abstract class OuroEventStoreTest
    {
        private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
        
        private readonly ILogger<IEventStore> _logger = Substitute.For<ILogger<IEventStore>>();

        private readonly UserCredentials _credentials = new UserCredentials("username", "password");

        private static async Task<IEventStoreConnection> InitializeOuro()
        {
            var connection = EventStoreConnection.Create (
                ConnectionSettings.Create()
                    .WithConnectionTimeoutOf(TimeSpan.FromSeconds(5))
                    .KeepReconnecting()
                    .KeepRetrying()
                    .UseConsoleLogger()
                    .Build(),
                new Uri ("tcp://admin:changeit@eventstore:1113")
            );
		    
            await connection.ConnectAsync();
            return connection;
        }

        private async Task<OuroEventStore> Store()
        {
            var ouro = await InitializeOuro();
            return new OuroEventStore(
                _logger,
                new OuroSourceFactory(
                    _loggerFactory,
                    ouro,
                    _credentials
                ),
                ouro,
                _credentials
            );
        }

        [Fact]
        public async Task ItCanInstantiateOuroStore()
        {
            (await Store()).ShouldNotBeNull();
        }
        
        [Fact]
        public void ItCanNotSaveEmptyStreamName()
        {
            Should.Throw<InvalidStreamException>(async () => (await Store()).Append<ExampleAggregate>("", new[] {
                new DomainMessage(0, new DateTime(), new object())
            }));
        }
        
        [Fact]
        public void ItCanNotSaveInvalidStreamName()
        {
            Should.Throw<InvalidStreamException>(async () => (await Store()).Append<ExampleAggregate>("@", new[] {
                new DomainMessage(0, new DateTime(), new object())
            }));
        }
        
        public class ExampleAggregate : EventSourced<ExampleAggregate>
        {
            public override string Identity()
            {
                return "identity";
            }
        }
    }
}