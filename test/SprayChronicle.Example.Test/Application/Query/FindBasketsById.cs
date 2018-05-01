using System;
using System.Threading.Tasks;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.QueryHandling;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.Example.Test.Application.Query
{
    public class FindBasketsById : QueryTestCase<Module,Basket>
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

        protected override Task<object> When(IQueryDispatcher dispatcher)
        {
            return dispatcher.Dispatch(
                new BasketById("basketId")
            );
        }

        protected override void Then(IValidate validator)
        {
            validator.Expect(
                new QueryBasketWithProducts.BasketWithProducts_v1(_basketId1, DateTime.Now),
                new QueryBasketWithProducts.BasketWithProducts_v1(_basketId2, DateTime.Now)
            );
        }

        [Fact]
        public override async Task Scenario()
        {
            await base.Scenario();
        }
    }
}