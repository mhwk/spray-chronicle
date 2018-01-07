using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using Shouldly;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
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
        public void ItLoadsMessages()
        {
            _persistence
                .Load<Basket>(Arg.Is("foo"))
                .Returns(new [] { new DomainMessage(0, new DateTime(), new BasketPickedUp("foo")) });
                
            new EventSourcedRepository<Basket>(_persistence)
                .Load("foo")
                .ShouldBeAssignableTo<PickedUpBasket>();
        }

        [Fact]
        public void ItIsOkIfNull()
        {
            _persistence
                .Load<Basket>(Arg.Is("foo"))
                .Returns(new [] { new DomainMessage(0, new DateTime(), new object {}) });
            
            new EventSourcedRepository<Basket>(_persistence)
                .Load("foo")
                .Should()
                .BeNull();
        }

        [Fact]
        public void ItFailsIfSpecificStateIsNull()
        {
            _persistence
                .Load<Basket>(Arg.Is("foo"))
                .Returns(new [] { new DomainMessage(0, new DateTime(), new object {}) });

            Should.Throw<InvalidStateException>(
                () => new EventSourcedRepository<Basket>(_persistence)
                    .Load<Basket>("foo")
            );
        }

        [Fact]
        public void ItIsOkIfDefaultIsNull()
        {
            _persistence
                .Load<Basket>(Arg.Is("foo"))
                .Returns(new [] { new DomainMessage(0, new DateTime(), new object {}) });
            
            new EventSourcedRepository<Basket>(_persistence)
                .LoadOrDefault<Basket>("foo")
                .Should()
                .BeNull();
        }
    }
}