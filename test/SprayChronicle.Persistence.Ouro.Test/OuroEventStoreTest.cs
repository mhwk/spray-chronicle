using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using FluentAssertions;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SprayChronicle.Example.Contracts.Events;
using SprayChronicle.Example.Domain;
using SprayChronicle.EventSourcing;
using SprayChronicle.Persistence.Ouro;

namespace SprayChronicle.Test.EventPersisting
{
    public class OuroEventStoreTest
    {
        /**
         * Sadly, due to lack embedded client for dotnet core, a lot is untestable.
         */

         public Mock<ILogger<IEventStore>> Logger = new Mock<ILogger<IEventStore>>();

         public Mock<IEventStoreConnection> EventStore = new Mock<IEventStoreConnection>();

         [Fact]
         public void ItCanInstantiateOuroPersister()
         {
             var persister = new OuroEventStore(Logger.Object, EventStore.Object, new UserCredentials("username", "password"));
             persister.Should().NotBeNull();
         }

         [Fact]
         public void ItCanNotSaveInvalidStreamName()
         {
             var persister = new OuroEventStore(Logger.Object, EventStore.Object, new UserCredentials("username", "password"));
             persister.Append<ExampleAggregate>("@", new DomainMessage[0] {});
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