using Xunit;
using SprayChronicle.Testing;
using SprayChronicle.Example.Contracts.Commands;
using SprayChronicle.Example.Contracts.Events;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class ItCanAddProductToBasket : EventSourcedTestCase<BasketHandler,Basket>
    {
        protected override object[] Given()
        {
            return new object[] {
                new BasketPickedUp("basketId")
            };
        }

        protected override object When()
        {
            return new AddProductToBasket("basketId", "productId");
        }

        protected override object[] Expect()
        {
            return new object[] {
                new ProductAddedToBasket("basketId", "productId")
            };
        }
    }
}
