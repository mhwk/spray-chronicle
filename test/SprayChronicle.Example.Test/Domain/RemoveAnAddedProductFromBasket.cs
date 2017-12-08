using SprayChronicle.Testing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class RemoveAnAddedProductFromBasket : EventSourcedTestCase<Module,Basket>
    {
        protected override Basket Given(Basket basket)
        {
            return Basket
                .PickUp(new BasketId("basketId"))
                .AddProduct(new ProductId("productId"));
        }

        protected override Basket When(Basket basket)
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
