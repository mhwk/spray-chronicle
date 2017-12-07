using System.Threading.Tasks;
using Xunit;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Testing.Test
{
    public class EventSourcedFixtureTest
    {
        private EventSourcedFixture<Module,Basket> Fixture()
        {
            return new EventSourcedFixture<Module,Basket>();
        }

        [Fact]
        public async Task ItCanTestInitialWhen()
        {
            (await Fixture()
                .When(basket => Basket.PickUp(new BasketId("basketId"))))
                .ExpectNoException()
                .Expect(new BasketPickedUp("basketId"));
        }

        [Fact]
        public async Task ItCanTestWhenAfterHistory()
        {
//            (await (await Fixture()
//                .Given(new BasketPickedUp("basketId")))
//                .When(basket => (basket as PickedUpBasket)?.AddProduct(new ProductId("productId"))))
//                .ExpectNoException()
//                .Expect(new ProductAddedToBasket("basketId", "productId"));
        }
    }
}
