using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;
using Xunit;

namespace SprayChronicle.Persistence.Ouro.Test
{
    public abstract class OuroEventStoreTest
    {
        /**
         * Sadly, due to lack embedded client for dotnet core, a lot is untestable.
         */

        private readonly Mock<ILogger<IEventStore>> _logger = new Mock<ILogger<IEventStore>>();

        private readonly Mock<IEventStoreConnection> _eventStore = new Mock<IEventStoreConnection>();

        [Fact]
        public void ItCanInstantiateOuroPersister()
        {
            var persister = new OuroEventStore(_logger.Object, _eventStore.Object, new UserCredentials("username", "password"), "Tenant");
            persister.Should().NotBeNull();
        }
        
        [Fact]
        public void ItCanNotSaveEmptyStreamName()
        {
            var persister = new OuroEventStore(_logger.Object, _eventStore.Object, new UserCredentials("username", "password"), "Tenant");
            Action append = () => persister.Append<ExampleAggregate>("", new[] {
                new DomainMessage(0, new DateTime(), new object{}.ToMessage())
            });
            append.ShouldThrow<InvalidStreamException>();
        }
        
        [Fact]
        public void ItCanNotSaveInvalidStreamName()
        {
            var persister = new OuroEventStore(_logger.Object, _eventStore.Object, new UserCredentials("username", "password"), "Tenant");
            Action append = () => persister.Append<ExampleAggregate>("@", new[] {
                new DomainMessage(0, new DateTime(), new object{}.ToMessage())
            });
            append.ShouldThrow<InvalidStreamException>();
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