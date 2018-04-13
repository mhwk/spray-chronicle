using System.Threading.Tasks;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Domain;
using SprayChronicle.QueryHandling;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.Example.Test.Application.Query
{
    public class FindAllNumerOfProductsInBaskets : QueryTestCase<Module>
    {
        protected override Task Given(TestStream stream)
        {
            return stream.Publish(
                new BasketPickedUp("basketId1"),
                new BasketPickedUp("basketId2")
            );
        }

        protected override Task<object> When(IQueryRouter processor)
        {
            return processor.Route(
                new NumberOfProductsInBaskets()
            );
        }

        protected override void Then(IValidate validator)
        {
            validator.Expect(
                new ProductInBasket("basketId1", 0),
                new ProductInBasket("basketId2", 0)
            );
        }

        [Fact]
        public override async Task Scenario()
        {
            await base.Scenario();
        }
    }
}