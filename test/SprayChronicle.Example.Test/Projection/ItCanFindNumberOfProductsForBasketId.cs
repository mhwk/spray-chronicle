using Xunit;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Model;
using SprayChronicle.Example.Domain;
using SprayChronicle.Testing;

namespace SprayChronicle.Example.Test.Projection
{
    public class ItCanFindNumberOfProductsForBasketId : ProjectionQueryTestCase<Module>
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

        [Fact]
        public override void Scenario()
        {
            base.Scenario();
        }
    }
}
