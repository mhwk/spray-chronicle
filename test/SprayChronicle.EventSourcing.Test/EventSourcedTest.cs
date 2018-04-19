using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.EventSourcing.Test
{
    public class EventSourcedTest
    {
        [Fact]
        public void ItProvidesDiff()
        {
            var diff = Basket
                .PickUp(new BasketId("foo"))
                .Diff();
            
            diff
                .Select(domainMessage => domainMessage.Payload.GetType())
                .ShouldBe(new object[] {
                    typeof(BasketPickedUp)
                });
        }

        [Fact]
        public void ItPatches()
        {
            var diff = Basket
                .PickUp(new BasketId("foo"))
                .AddProduct(new ProductId("bar"))
                .Diff();
            
            diff
                .Select(domainMessage => domainMessage.Payload.GetType())
                .ShouldBe(new object[] {
                    typeof(BasketPickedUp),
                    typeof(ProductAddedToBasket)
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
                .ToArray()
                .ShouldBe(new [] {0L, 1L, 2L});
        }

        [Fact]
        public async Task ItCalculatesSequenceAfterPatch()
        {
            var source = new TestSource<Basket>();
            await source.Publish(new BasketPickedUp("foo"));
            
            var aggregate = (PickedUpBasket) await Basket.Patch(source);
            aggregate
                .AddProduct(new ProductId("bar"))
                .AddProduct(new ProductId("bar"))
                .Diff()
                .Select(domainMessage => domainMessage.Sequence)
                .ToArray()
                .ShouldBe(new [] {1L, 2L});
        }

        [Fact]
        public async Task ItPatchesGracefully()
        {
            var source = new TestSource<Basket>();
            
            await source.Publish(new BasketPickedUp("foo"));
            await source.Publish(new UnknownBasketEvent());
            await Basket.Patch(source);
        }

        [Fact]
        public async Task ItCalculatesSequenceAfterUnknownPatch()
        {
            var source = new TestSource<Basket>();
            
            await source.Publish(new BasketPickedUp("foo"));
            await source.Publish(new UnknownBasketEvent());
            await Basket.Patch(source);
            
            var aggregate = (PickedUpBasket) await Basket.Patch(source);
            
            aggregate
                .AddProduct(new ProductId("bar"))
                .AddProduct(new ProductId("bar"))
                .Diff()
                .Select(domainMessage => domainMessage.Sequence)
                .ToArray()
                .ShouldBe(new [] {2L, 3L});
        }

        private sealed class UnknownBasketEvent
        {

        }
    }
}