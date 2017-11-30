using SprayChronicle.Testing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class RemoveAnAddedProductFromBasket : EventSourcedTestCase<Module,Basket>
    {
        protected override object[] Given()
        {
            return new object[] {
                new BasketPickedUp("basketId"),
                new ProductAddedToBasket("basketId", "productId")
            };
        }

        protected override Basket When(Basket basket)
        {
            return (basket as PickedUpBasket)?.RemoveProduct(new ProductId("productId"));
        }

        protected override object[] Expect()
        {
            return new object[] {
                new ProductRemovedFromBasket("basketId", "productId")
            };
        }
    }
}
