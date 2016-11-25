using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Contracts.Events;

namespace SprayChronicle.Test.EvenSourcing
{
    public class EventSourcedTest
    {
        [Fact]
        public void ItProvidesDiff()
        {
            Basket
                .PickUp(new BasketId("foo"))
                .Diff()
                .Select(domainMessage => domainMessage.Payload)
                .ShouldBeEquivalentTo(new object[1] {new BasketPickedUp("foo")});
        }

        [Fact]
        public void ItPatches()
        {
            Basket
                .PickUp(new BasketId("foo"))
                .AddProduct(new ProductId("bar"))
                .Diff()
                .Select(domainMessage => domainMessage.Payload)
                .ShouldBeEquivalentTo(new object[2] {
                    new BasketPickedUp("foo"),
                    new ProductAddedToBasket("foo", "bar")
                });
        }

        [Fact]
        public void ItCalculatesSequence()
        {
            Basket
                .PickUp(new BasketId("foo"))
                .AddProduct(new ProductId("bar"))
                .AddProduct(new ProductId("bar"))
                .Diff()
                .Select(domainMessage => domainMessage.Sequence)
                .ShouldBeEquivalentTo(new int[] {0, 1, 2});
        }

        [Fact]
        public void ItCalculatesSequenceAfterPatch()
        {
            var aggregate = (PickedUpBasket) Basket.Patch(new DomainMessage[] {
                new DomainMessage(
                    0,
                    new DateTime(),
                    new BasketPickedUp("foo")
                )
            });
            aggregate
                .AddProduct(new ProductId("bar"))
                .AddProduct(new ProductId("bar"))
                .Diff()
                .Select(domainMessage => domainMessage.Sequence)
                .ShouldBeEquivalentTo(new int[] {1, 2});
        }

        [Fact]
        public void ItDoesNotPatchGracefully()
        {
            var aggregate = (PickedUpBasket) Basket.Patch(new DomainMessage[] {
                new DomainMessage(
                    0,
                    new DateTime(),
                    new BasketPickedUp("foo")
                ),
                new DomainMessage(
                    1,
                    new DateTime(),
                    new UnknownBasketEvent()
                ),
            });
        }

        [Fact]
        public void ItCalculatesSequenceAfterUnknownPatch()
        {
            var aggregate = (PickedUpBasket) Basket.Patch(new DomainMessage[] {
                new DomainMessage(
                    0,
                    new DateTime(),
                    new BasketPickedUp("foo")
                ),
                new DomainMessage(
                    1,
                    new DateTime(),
                    new UnknownBasketEvent()
                ),
            });
            aggregate
                .AddProduct(new ProductId("bar"))
                .AddProduct(new ProductId("bar"))
                .Diff()
                .Select(domainMessage => domainMessage.Sequence)
                .ShouldBeEquivalentTo(new int[] {2, 3});
        }

        public sealed class UnknownBasketEvent
        {

        }
    }
}