using System.Threading.Tasks;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Model;
using SprayChronicle.Example.Domain;
using SprayChronicle.QueryHandling;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.Example.Test.Application.Query
{
    public class FindAllNumerOfProductsInBaskets : QueryTestCase<Module>
    {
        protected override object[] Given()
        {
            return new object[] {
                new BasketPickedUp("basketId1"),
                new BasketPickedUp("basketId2"),
            };
        }

        protected override Task<object> When(IProcessQueries processor)
        {
            return processor.Process(new NumberOfProductsInBaskets());
        }

        protected override object[] Expect()
        {
            return new object[] {
                new NumberOfProductsInBasket("basketId1", 0),
                new NumberOfProductsInBasket("basketId2", 0),
            };
        }

        [Fact]
        public override async Task Scenario()
        {
            await base.Scenario();
        }
    }
}