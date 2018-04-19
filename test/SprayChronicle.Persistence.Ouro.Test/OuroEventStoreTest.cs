using System;
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

        private readonly IEventStoreConnection _eventStore = Substitute.For<IEventStoreConnection>();

        private readonly UserCredentials _credentials = new UserCredentials("username", "password");

        private OuroEventStore Store()
        {
            return new OuroEventStore(
                _logger,
                new OuroSourceFactory(
                    _loggerFactory,
                    _eventStore,
                    _credentials
                ),
                _eventStore,
                _credentials
            );
        }

        [Fact]
        public void ItCanInstantiateOuroStore()
        {
            Store().ShouldNotBeNull();
        }
        
        [Fact]
        public void ItCanNotSaveEmptyStreamName()
        {
            Should.Throw<InvalidStreamException>(() => Store().Append<ExampleAggregate>("", new[] {
                new DomainMessage(0, new DateTime(), new object())
            }));
        }
        
        [Fact]
        public void ItCanNotSaveInvalidStreamName()
        {
            Should.Throw<InvalidStreamException>(() => Store().Append<ExampleAggregate>("@", new[] {
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