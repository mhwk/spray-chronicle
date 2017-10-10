using Xunit;
using SprayChronicle.Testing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Model;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.Example.Test.Projection
{
    public class ItCanFindAllNumerOfProductsInBaskets : ProjectionQueryTestCase<ExampleModule>
    {
        protected override object[] Given()
        {
            return new object[] {
                new BasketPickedUp("basketId1"),
                new BasketPickedUp("basketId2"),
            };
        }

        protected override object When()
        {
            return new NumberOfProductsInBaskets();
        }

        protected override object[] Expect()
        {
            return new object[] {
                new NumberOfProductsInBasket("basketId1", 0),
                new NumberOfProductsInBasket("basketId2", 0),
            };
        }

        [Fact]
        public override void ItAcceptsScenario()
        {
            base.ItAcceptsScenario();
        }
    }
}