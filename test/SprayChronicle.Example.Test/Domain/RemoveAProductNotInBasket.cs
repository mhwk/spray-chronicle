using SprayChronicle.Testing;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class RemoveAProductNotInBasket : EventSourcedTestCase<Module,Basket>
    {
        protected override Basket Given(Basket basket)
        {
            return Basket.PickUp(new BasketId("basketId"));
        }

        protected override Basket When(Basket basket)
        {
            return (basket as PickedUpBasket).RemoveProduct(new ProductId("productId"));
        }

        protected override void Then(IValidate validator)
        {
            validator.ExpectException(typeof(ProductNotInBasketException));
        }
    }
}
