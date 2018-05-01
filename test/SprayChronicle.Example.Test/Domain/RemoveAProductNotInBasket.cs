using System.Threading.Tasks;
using SprayChronicle.Testing;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class RemoveAProductNotInBasket : EventSourcedTestCase<Module,Basket>
    {
        protected override Task<Basket> Given()
        {
            return Basket.PickUp(new BasketId("basketId"));
        }

        protected override Task<Basket> When(Basket basket)
        {
            return ((PickedUpBasket) basket).RemoveProduct(new ProductId("productId"));
        }

        protected override void Then(IValidate validator)
        {
            validator.ExpectException(typeof(ProductNotInBasketException));
        }
    }
}
