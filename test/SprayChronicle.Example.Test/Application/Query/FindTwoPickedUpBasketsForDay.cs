using System.Threading.Tasks;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.State;
using SprayChronicle.QueryHandling;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.Example.Test.Application.Query
{
    public class FindTwoPickedUpBasketsForDay : QueryTestCase<Module>
    {
        protected override Task Given(TestStream stream)
        {
            return stream
                .Epochs(
                    "2016-01-01T00:00:00+00:00",
                    "2016-01-01T00:00:00+00:00",
                    "2016-01-02T00:00:00+00:00"
                )
                .Publish(
                    new BasketPickedUp("basketId1"),
                    new BasketPickedUp("basketId2"),
                    new BasketPickedUp("basketId3")
                );
        }

        protected override Task<object> When(IProcessQueries processor)
        {
            return processor.Process(
                new PickedUpBasketsOnDay("2016-01-01")
            );
        }

        protected override object[] Expect()
        {
            return new object[] {
                new PickedUpBasketsPerDay("2016-01-01", 2)
            };
        }

        [Fact]
        public override async Task Scenario()
        {
            await base.Scenario();
        }
    }
}