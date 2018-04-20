using System.Threading.Tasks;
using SprayChronicle.Testing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class RemoveAnAddedProductFromBasket : EventSourcedTestCase<Module,Basket>
    {
        protected override async Task<Basket> Given(Basket basket)
        {
            basket = await Basket.PickUp(new BasketId("basketId"));
            basket = await (basket as PickedUpBasket).AddProduct(new ProductId("productId"));
            
            return basket;
        }

        protected override Task<Basket> When(Basket basket)
        {
            return (basket as PickedUpBasket)?.RemoveProduct(new ProductId("productId"));
        }

        protected override void Then(IValidate validator)
        {
            validator.Expect(
                new ProductRemovedFromBasket("basketId", "productId")
            );
        }
    }
}
