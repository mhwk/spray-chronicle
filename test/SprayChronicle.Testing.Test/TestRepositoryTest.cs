using System;
using Xunit;
using FluentAssertions;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Contracts.Events;

namespace SprayChronicle.Testing.Test
{
    public class TestRepositoryTest
    {
        [Fact]
        public void ItWillFailIfConcurrencyDetected()
        {
            TestRepository<Basket> repo = new TestRepository<Basket>();
            repo.History(new DomainMessage(0, new DateTime(), new BasketPickedUp("foo")));
            Action e = () => repo.Save(Basket.PickUp(new BasketId("foo")));
            e.ShouldThrow<ConcurrencyException>();
        }
    }
}