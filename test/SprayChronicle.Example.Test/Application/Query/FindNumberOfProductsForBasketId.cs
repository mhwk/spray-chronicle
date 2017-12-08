using System.Threading.Tasks;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.State;
using SprayChronicle.QueryHandling;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.Example.Test.Application.Query
{
    public class FindNumberOfProductsForBasketId : QueryTestCase<Module>
    {
        protected override Task Given(TestStream stream)
        {
            return stream.Publish(
                new BasketPickedUp("basketId"),
                new ProductAddedToBasket("basketId", "productId")
            );
        }

        protected override Task<object> When(IProcessQueries processor)
        {
            return processor.Process(new Example.Application.NumberOfProductsForBasketId("basketId"));
        }

        protected override void Then(IValidate validator)
        {
            validator.Expect(
                new NumberOfProductsInBasket("basketId", 1)
            );
        }

        [Fact]
        public override async Task Scenario()
        {
            await base.Scenario();
        }
    }
}
