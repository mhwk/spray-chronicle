using System.Threading.Tasks;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Model;
using SprayChronicle.Example.Domain;
using SprayChronicle.QueryHandling;
using SprayChronicle.Testing;

namespace SprayChronicle.Example.Test.Application.Query
{
    public class PagedNumberOfProductsInBasket : QueryTestCase<Module>
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
            return processor.Process(new Example.Application.PagedNumberOfProductsInBasket(2, 1));
        }

        protected override object[] Expect()
        {
            return new object[] {
                new PagedResult<NumberOfProductsInBasket>(
                    new [] {new NumberOfProductsInBasket("basketId2", 0)},
                    2,
                    1,
                    2
                )
            };
        }
    }
}