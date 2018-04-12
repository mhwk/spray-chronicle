using System.Threading.Tasks;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.State;
using SprayChronicle.QueryHandling;
using SprayChronicle.Testing;

namespace SprayChronicle.Example.Test.Application.Query
{
    public class PagedNumberOfProductsInBasket : QueryTestCase<Module>
    {
        protected override Task Given(TestStream stream)
        {
            return stream.Publish(
                new BasketPickedUp("basketId1"),
                new BasketPickedUp("basketId2")
            );
        }

        protected override Task<object> When(IProcessQueries processor)
        {
            return processor.Process(new Example.Application.PagedNumberOfProductsInBasket(2, 1));
        }

        protected override void Then(IValidate validate)
        {
            validate.Expect(
                new PagedResult<ProductInBasket>(
                    new [] {new ProductInBasket("basketId2", 0)},
                    2,
                    1,
                    2
                )
            );
        }
    }
}