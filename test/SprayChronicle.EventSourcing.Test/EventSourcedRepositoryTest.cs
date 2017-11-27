using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.MessageHandling;
using Xunit;

namespace SprayChronicle.EventSourcing.Test
{
    public class EventSourcedRepositoryTest
    {
        private readonly Mock<IEventStore> _persistence = new Mock<IEventStore>();

        [Fact]
        public void ItAppendsMessages()
        {
            new EventSourcedRepository<Basket>(_persistence.Object).Save(
                Basket.PickUp(new BasketId("foo"))
            );
            
            _persistence.Verify(p => p.Append<Basket>(
                It.Is<string>(i => i == "foo"),
                It.Is<IEnumerable<DomainMessage>>(i => IsEqual(i.Select(dm => dm.Payload), new object[] { new InstanceMessage(new BasketPickedUp("foo")),  })
            )));
        }

        [Fact]
        public void ItLoadsMessages()
        {
            _persistence.Setup(p => p
                .Load<Basket>(It.Is<string>(i => i == "foo")))
                .Returns(new DomainMessage[] { new DomainMessage(0, new DateTime(), new InstanceMessage(new BasketPickedUp("foo"))) });
                
            new EventSourcedRepository<Basket>(_persistence.Object).Load("foo").Should().BeAssignableTo<PickedUpBasket>();
        }

        [Fact]
        public void ItIsOkIfNull()
        {
            _persistence.Setup(p => p
                .Load<Basket>(It.Is<string>(i => i == "foo")))
                .Returns(new DomainMessage[] { new DomainMessage(0, new DateTime(), new InstanceMessage(new object {})) });
            new EventSourcedRepository<Basket>(_persistence.Object).Load("foo").Should().BeNull();
        }

        [Fact]
        public void ItFailsIfSpecificStateIsNull()
        {
            _persistence.Setup(p => p
                .Load<Basket>(It.Is<string>(i => i == "foo")))
                .Returns(new DomainMessage[] { new DomainMessage(0, new DateTime(), new InstanceMessage(new object {})) });
            Action action = () => new EventSourcedRepository<Basket>(_persistence.Object).Load<Basket>("foo");
            action.ShouldThrow<InvalidStateException>();
        }

        [Fact]
        public void ItIsOkIfDefaultIsNull()
        {
            _persistence.Setup(p => p
                .Load<Basket>(It.Is<string>(i => i == "foo")))
                .Returns(new DomainMessage[] { new DomainMessage(0, new DateTime(), new InstanceMessage(new object {})) });
            new EventSourcedRepository<Basket>(_persistence.Object).LoadOrDefault<Basket>("foo").Should().BeNull();
        }

        private static bool IsEqual(object first, object second)
        {
            try {
                first.ShouldBeEquivalentTo(second);
            } catch (Exception) {
                return false;
            }
            return true;
        }
    }
}