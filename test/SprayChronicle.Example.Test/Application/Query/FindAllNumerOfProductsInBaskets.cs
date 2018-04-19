using System;
using System.Threading.Tasks;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.State;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.QueryHandling;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.Example.Test.Application.Query
{
    public class FindAllNumerOfProductsInBaskets : QueryTestCase<Module,Basket>
    {
        protected override Task Given(TestSource<Basket> source)
        {
            return source.Publish(
                new BasketPickedUp("basketId1"),
                new BasketPickedUp("basketId2")
            );
        }

        protected override Task<object> When(QueryRouter processor)
        {
            return processor.Route(
                new NumberOfProductsInBaskets()
            );
        }

        protected override void Then(IValidate validator)
        {
            validator.Expect(
                new BasketWithProducts_v1("basketId1", DateTime.Now),
                new BasketWithProducts_v1("basketId2", DateTime.Now)
            );
        }

        [Fact]
        public override async Task Scenario()
        {
            await base.Scenario();
        }
    }
}