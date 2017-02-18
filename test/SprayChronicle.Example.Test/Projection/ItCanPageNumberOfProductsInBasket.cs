using Xunit;
using System.Collections.Generic;
using System.Linq;
using SprayChronicle.QueryHandling;
using SprayChronicle.Testing;
using SprayChronicle.Example.Contracts.Events;
using SprayChronicle.Example.Contracts.Queries;
using SprayChronicle.Example.Projection;

namespace SprayChronicle.Example.Test.Projection
{
    public class ItCanPageNumberOfProductsInBasket : ProjectionQueryTestCase<ExampleProjectionModule>
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
            return new PagedNumberOfProductsInBasket(2, 1);
        }

        protected override object[] Expect()
        {
            return new object[] {
                new PagedResult<NumberOfProductsInBasket>(
                    new NumberOfProductsInBasket[] {new NumberOfProductsInBasket("basketId2", 0)},
                    2,
                    1,
                    2
                )
            };
        }
    }
}