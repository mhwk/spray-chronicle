using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.EventSourcing.Test
{
    public class EventSourcedRepositoryTest
    {
        private readonly IEventStore _persistence = Substitute.For<IEventStore>();

        [Fact]
        public void ItAppendsMessages()
        {
            new EventSourcedRepository<Basket>(_persistence)
                .Save(Basket.PickUp(new BasketId("foo")));
            
            _persistence
                .Received()
                .Append<Basket>(Arg.Is("foo"), Arg.Any<IEnumerable<IDomainMessage>>());
        }

        [Fact]
        public async Task ItLoadsMessages()
        {
            var source = new TestSource<Basket>();
            await source.Publish(new BasketPickedUp("foo"));
            
            _persistence
                .Load<Basket>(Arg.Is("foo"))
                .Returns(source);
                
            new EventSourcedRepository<Basket>(_persistence)
                .Load("foo")
                .ShouldBeAssignableTo<PickedUpBasket>();
        }

        [Fact]
        public async Task ItIsOkIfNull()
        {
            var source = new TestSource<Basket>();
            await source.Publish(new object());
            
            _persistence
                .Load<Basket>(Arg.Is("foo"))
                .Returns(source);
            
            new EventSourcedRepository<Basket>(_persistence)
                .Load("foo")
                .ShouldBeNull();
        }

        [Fact]
        public async Task ItFailsIfSpecificStateIsNull()
        {
            var source = new TestSource<Basket>();
            await source.Publish(new object());
            
            _persistence
                .Load<Basket>(Arg.Is("foo"))
                .Returns(source);

            await Should.ThrowAsync<InvalidStateException>(
                new EventSourcedRepository<Basket>(_persistence).Load<Basket>("foo")
            );
        }

        [Fact]
        public async Task ItIsOkIfDefaultIsNull()
        {
            var source = new TestSource<Basket>();
            await source.Publish(new object());
            
            _persistence
                .Load<Basket>(Arg.Is("foo"))
                .Returns(source);
            
            new EventSourcedRepository<Basket>(_persistence)
                .LoadOrDefault<Basket>("foo")
                .ShouldBeNull();
        }
    }
}