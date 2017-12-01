using SprayChronicle.Testing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class AddAProductToPickedUpBasket : EventSourcedTestCase<Module,Basket>
    {
        protected override object[] Given()
        {
            return new object[] {
                new BasketPickedUp("basketId")
            };
        }

        protected override Basket When(Basket basket)
        {
            return (basket as PickedUpBasket)?.AddProduct(new ProductId("productId"));
        }

        protected override object[] Expect()
        {
            return new object[] {
                new ProductAddedToBasket("basketId", "productId")
            };
        }
    }
}