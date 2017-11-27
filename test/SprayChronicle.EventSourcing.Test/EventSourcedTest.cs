using System;
using System.Linq;
using FluentAssertions;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.MessageHandling;
using Xunit;

namespace SprayChronicle.EventSourcing.Test
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
                .ShouldAllBeEquivalentTo(new [] {new InstanceMessage(new BasketPickedUp("foo"))});
        }

        [Fact]
        public void ItPatches()
        {
            Basket
                .PickUp(new BasketId("foo"))
                .AddProduct(new ProductId("bar"))
                .Diff()
                .Select(domainMessage => domainMessage.Payload)
                .ShouldAllBeEquivalentTo(new [] {
                    new InstanceMessage(new BasketPickedUp("foo")),
                    new InstanceMessage(new ProductAddedToBasket("foo", "bar"))
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
                .ShouldAllBeEquivalentTo(new [] {0, 1, 2});
        }

        [Fact]
        public void ItCalculatesSequenceAfterPatch()
        {
            var aggregate = (PickedUpBasket) Basket.Patch(new [] {
                new DomainMessage(
                    0,
                    new DateTime(),
                    new InstanceMessage(new BasketPickedUp("foo"))
                )
            });
            aggregate
                .AddProduct(new ProductId("bar"))
                .AddProduct(new ProductId("bar"))
                .Diff()
                .Select(domainMessage => domainMessage.Sequence)
                .ShouldAllBeEquivalentTo(new [] {1, 2});
        }

        [Fact]
        public void ItDoesNotPatchGracefully()
        {
            var aggregate = (PickedUpBasket) Basket.Patch(new DomainMessage[] {
                new DomainMessage(
                    0,
                    new DateTime(),
                    new InstanceMessage(new BasketPickedUp("foo"))
                ),
                new DomainMessage(
                    1,
                    new DateTime(),
                    new InstanceMessage(new UnknownBasketEvent())
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
                    new InstanceMessage(new BasketPickedUp("foo"))
                ),
                new DomainMessage(
                    1,
                    new DateTime(),
                    new InstanceMessage(new UnknownBasketEvent())
                ),
            });
            aggregate
                .AddProduct(new ProductId("bar"))
                .AddProduct(new ProductId("bar"))
                .Diff()
                .Select(domainMessage => domainMessage.Sequence)
                .ShouldAllBeEquivalentTo(new long[] {2, 3});
        }

        private sealed class UnknownBasketEvent
        {

        }
    }
}