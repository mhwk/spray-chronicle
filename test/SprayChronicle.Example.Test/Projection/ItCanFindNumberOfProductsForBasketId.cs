using Xunit;
using SprayChronicle.Example.Contracts.Events;
using SprayChronicle.Example.Contracts.Queries;
using SprayChronicle.Example.Projection;
using SprayChronicle.Testing;

namespace SprayChronicle.Example.Test.Projection
{
    public class ItCanFindNumberOfProductsForBasketId : ProjectionQueryTestCase<ExampleProjectionModule>
    {
        protected override object[] Given()
        {
            return new object[] {
                new BasketPickedUp("basketId"),
                new ProductAddedToBasket("basketId", "productId")
            };
        }

        protected override object When()
        {
            return new NumberOfProductsForBasketId("basketId");
        }

        protected override object[] Expect()
        {
            return new object[] {
                new NumberOfProductsInBasket("basketId", 1)
            };
        }
    }
}
