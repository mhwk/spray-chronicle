using Xunit;
using System.Collections.Generic;
using System.Linq;
using SprayChronicle.QueryHandling;
using SprayChronicle.Testing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Model;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.Example.Test.Projection
{
    public class ItCanPageNumberOfProductsInBasket : ProjectionQueryTestCase<ExampleModule>
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