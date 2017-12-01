using System.Threading.Tasks;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Model;
using SprayChronicle.Example.Domain;
using SprayChronicle.QueryHandling;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.Example.Test.Application.Query
{
    public class FindNumberOfProductsForBasketId : QueryTestCase<Module>
    {
        protected override object[] Given()
        {
            return new object[] {
                new BasketPickedUp("basketId"),
                new ProductAddedToBasket("basketId", "productId")
            };
        }

        protected override Task<object> When(IProcessQueries processor)
        {
            return processor.Process(new Example.Application.NumberOfProductsForBasketId("basketId"));
        }

        protected override object[] Expect()
        {
            return new object[] {
                new NumberOfProductsInBasket("basketId", 1)
            };
        }

        [Fact]
        public override async Task Scenario()
        {
            await base.Scenario();
        }
    }
}
