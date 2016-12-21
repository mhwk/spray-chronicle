using Xunit;
using SprayChronicle.Testing;
using SprayChronicle.Example.Contracts.Events;
using SprayChronicle.Example.Contracts.Queries;
using SprayChronicle.Example.Projection;

namespace SprayChronicle.Example.Test.Projection
{
    public class ItCanFindAllNumerOfProductsInBaskets : ProjectionQueryTestCase<ExampleProjectionModule>
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