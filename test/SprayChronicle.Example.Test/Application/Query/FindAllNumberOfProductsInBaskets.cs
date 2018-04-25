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
    public class FindAllNumberOfProductsInBaskets : QueryTestCase<Module,Basket>
    {
        private readonly string _basketId1 = Guid.NewGuid().ToString();
        private readonly string _basketId2 = Guid.NewGuid().ToString();
        
        protected override Task Given(TestSource<Basket> source)
        {
            return source.Publish(
                new BasketPickedUp(_basketId1),
                new BasketPickedUp(_basketId2)
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
                new BasketWithProducts_v2(_basketId2, DateTime.Now),
                new BasketWithProducts_v2(_basketId2, DateTime.Now)
            );
        }

        [Fact]
        public override async Task Scenario()
        {
            await base.Scenario();
        }
    }
}