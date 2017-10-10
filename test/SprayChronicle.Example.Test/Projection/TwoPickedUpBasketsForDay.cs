using System;
using System.Globalization;
using SprayChronicle.Testing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Model;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.Example.Test.Projection
{
    public class TwoPickedUpBasketsForDay : ProjectionQueryTestCase<ExampleModule>
    {
        protected override DateTime[] Epoch()
        {
            return new DateTime[] {
                DateTime.ParseExact("2016-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                DateTime.ParseExact("2016-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                DateTime.ParseExact("2016-01-02", "yyyy-MM-dd", CultureInfo.InvariantCulture),
            };
        }

        protected override object[] Given()
        {
            return new object[] {
                new BasketPickedUp("basketId1"),
                new BasketPickedUp("basketId2"),
                new BasketPickedUp("basketId3"),
            };
        }

        protected override object When()
        {
            return new PickedUpBasketsOnDay("2016-01-01");
        }

        protected override object[] Expect()
        {
            return new object[] {
                new PickedUpBasketsPerDay("2016-01-01", 2)
            };
        }
    }
}