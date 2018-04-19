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
        public async Task ItProvidesDiff()
        {
            var basket = (PickedUpBasket) await Basket.PickUp("foo");
            
            basket.Diff()
                .Select(domainMessage => domainMessage.Payload.GetType())
                .ShouldBe(new object[] {
                    typeof(BasketPickedUp)
                });
        }

        [Fact]
        public async Task ItPatches()
        {
            var basket = (PickedUpBasket) await Basket.PickUp("foo");
            basket = (PickedUpBasket) await basket.AddProduct(new ProductId("bar"));
            
            basket.Diff()
                .Select(domainMessage => domainMessage.Payload.GetType())
                .ShouldBe(new object[] {
                    typeof(BasketPickedUp),
                    typeof(ProductAddedToBasket)
                });
        }

        [Fact]
        public async Task ItCalculatesSequence()
        {
            var basket = (PickedUpBasket) await Basket.PickUp("foo");
            basket = (PickedUpBasket) await basket.AddProduct(new ProductId("bar"));
            basket = (PickedUpBasket) await basket.AddProduct(new ProductId("bar"));
            
            basket.Diff()
                .Select(domainMessage => domainMessage.Sequence)
                .ToArray()
                .ShouldBe(new [] {0L, 1L, 2L});
        }

        [Fact]
        public async Task ItCalculatesSequenceAfterPatch()
        {
            var source = new TestSource<Basket>();
            await source.Publish(new BasketPickedUp("foo"));
            
            var basket = (PickedUpBasket) await Basket.Patch(source);
            basket = (PickedUpBasket) await basket.AddProduct(new ProductId("bar"));
            basket = (PickedUpBasket) await basket.AddProduct(new ProductId("bar"));
            
            basket.Diff()
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
            
            var basket = (PickedUpBasket) await Basket.Patch(source);
            basket = (PickedUpBasket) await basket.AddProduct(new ProductId("bar"));
            basket = (PickedUpBasket) await basket.AddProduct(new ProductId("bar"));
            
            basket.Diff()
                .Select(domainMessage => domainMessage.Sequence)
                .ToArray()
                .ShouldBe(new [] {2L, 3L});
        }

        private sealed class UnknownBasketEvent
        {

        }
    }
}