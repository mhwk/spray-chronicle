using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using FluentAssertions;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SprayChronicle.EventSourcing;
using SprayChronicle.Persistence.Ouro;

namespace SprayChronicle.Test.EventPersisting
{
    public class OuroEventStoreTest
    {
        /**
         * Sadly, due to lack embedded client for dotnet core, a lot is untestable.
         */

         public readonly Mock<ILogger<IEventStore>> Logger = new Mock<ILogger<IEventStore>>();

         public readonly Mock<IEventStoreConnection> EventStore = new Mock<IEventStoreConnection>();

         [Fact]
         public void ItCanInstantiateOuroPersister()
         {
             var persister = new OuroEventStore(Logger.Object, EventStore.Object, new UserCredentials("username", "password"), "Tenant");
             persister.Should().NotBeNull();
         }

         [Fact]
         public void ItCanNotSaveEmptyStreamName()
         {
             var persister = new OuroEventStore(Logger.Object, EventStore.Object, new UserCredentials("username", "password"), "Tenant");
             Action append = () => persister.Append<ExampleAggregate>("", new[] {
                 new DomainMessage(0, new DateTime(), new object{})
             });
             append.ShouldThrow<InvalidStreamException>();
         }

         [Fact]
         public void ItCanNotSaveInvalidStreamName()
         {
             var persister = new OuroEventStore(Logger.Object, EventStore.Object, new UserCredentials("username", "password"), "Tenant");
             Action append = () => persister.Append<ExampleAggregate>("@", new[] {
                 new DomainMessage(0, new DateTime(), new object{})
             });
             append.ShouldThrow<InvalidStreamException>();
         }

        //  [Fact]
        //  public void ItAppendsEvents()
        //  {
        //      var persister = new OuroEventStore(EventStore.Object);
        //      persister.Append<ExampleAggregate>("foo", new DomainMessage[] {
        //          new DomainMessage(10, new DateTime(), new ExampleModified("foo"))
        //      });

        //     EventStore.Verify(e => e.AppendToStreamAsync(
        //         It.Is<string>(i => i == "SprayChronicle-foo"),
        //         It.Is<int>(i => i == 10),
        //         It.IsAny<EventData[]>(),
        //         It.Is<UserCredentials>(uc => uc == null)
        //     ));
        //  }

        public class ExampleAggregate : EventSourced<ExampleAggregate>
        {
            public override string Identity()
            {
                return "identity";
            }
        }
    }
}