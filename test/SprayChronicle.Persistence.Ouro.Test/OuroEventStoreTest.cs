using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SprayChronicle.Example.Contract.Event;
using SprayChronicle.Example.Domain;
using SprayChronicle.EventSourcing;
using SprayChronicle.Persistence.Ouro;

namespace SprayChronicle.Test.EventPersisting
{
    public class OuroEventStoreTest
    {
        /**
         * Sadly, due to lack of dotnet core support for EventStore, a lot is untestable.
         */

         public Mock<IEventStoreConnection> EventStore = new Mock<IEventStoreConnection>();

         [Fact]
         public void ItCanInstantiateOuroPersister()
         {
             var persister = new OuroEventStore(EventStore.Object);
             persister.Should().NotBeNull();
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
    }
}