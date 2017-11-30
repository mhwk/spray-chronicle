using System;
using System.Globalization;
using System.Threading.Tasks;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Model;
using SprayChronicle.Example.Domain;
using SprayChronicle.QueryHandling;
using SprayChronicle.Testing;

namespace SprayChronicle.Example.Test.Application.Query
{
    public class FindTwoPickedUpBasketsForDay : QueryTestCase<Module>
    {
        protected override DateTime[] Epoch()
        {
            return new [] {
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

        protected override Task<object> When(IProcessQueries processor)
        {
            return processor.Process(new PickedUpBasketsOnDay("2016-01-01"));
        }

        protected override object[] Expect()
        {
            return new object[] {
                new PickedUpBasketsPerDay("2016-01-01", 2)
            };
        }
    }
}