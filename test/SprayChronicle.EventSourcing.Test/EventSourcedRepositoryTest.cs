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
        public async Task ItAppendsMessages()
        {
            await new EventSourcedRepository<Basket>(_persistence)
                .Save(await Basket.PickUp(new BasketId("foo")), new TestEnvelope());
            
            await _persistence
                .Received()
                .Append<Basket>(Arg.Is("foo"), Arg.Any<IEnumerable<IDomainEnvelope>>());
        }

        [Fact]
        public async Task ItLoadsMessages()
        {
            var source = new TestSource<Basket>();
            await source.Publish(new BasketPickedUp("foo"));
            source.Complete();
            
            _persistence
                .Load<Basket>(Arg.Is("foo"), "idempotencyId")
                .Returns(source);
                
            var result = await new EventSourcedRepository<Basket>(_persistence).Load("foo", "idempotencyId");
            result.ShouldBeAssignableTo<PickedUpBasket>();
        }

        [Fact]
        public async Task ItIsOkIfNull()
        {
            var source = new TestSource<Basket>();
            await source.Publish(new object());
            source.Complete();
            
            _persistence
                .Load<Basket>(Arg.Is("foo"), Arg.Any<string>())
                .Returns(source);
            
            var result = await new EventSourcedRepository<Basket>(_persistence).Load("foo", "idempotencyId");
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ItFailsIfSpecificStateIsNull()
        {
            var source = new TestSource<Basket>();
            await source.Publish(new object());
            source.Complete();
            
            _persistence
                .Load<Basket>(Arg.Is("foo"), "idempotencyId")
                .Returns(source);

            Should.Throw<InvalidStateException>(
                () => new EventSourcedRepository<Basket>(_persistence).Load<Basket>("foo", "idempotencyId")
            );
        }

        [Fact]
        public async Task ItIsOkIfDefaultIsNull()
        {
            var source = new TestSource<Basket>();
            await source.Publish(new object());
            source.Complete();
            
            _persistence
                .Load<Basket>(Arg.Is("foo"), Arg.Any<string>())
                .Returns(source);
            
            var result = await new EventSourcedRepository<Basket>(_persistence).LoadOrDefault<Basket>("foo", "idempotencyId");
            result.ShouldBeNull();
        }
    }
}