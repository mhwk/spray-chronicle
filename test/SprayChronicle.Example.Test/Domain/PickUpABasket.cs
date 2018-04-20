using System.Threading.Tasks;
using SprayChronicle.Testing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using Xunit;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class PickUpABasket : EventSourcedTestCase<Module,Basket>
    {
        protected override Task<Basket> Given(Basket sourced)
        {
            return null;
        }

        protected override Task<Basket> When(Basket basket)
        {
            return Basket.PickUp(new BasketId("basketId"));
        }

        protected override void Then(IValidate validator)
        {
            validator.Expect(
                new BasketPickedUp("basketId")
            );
        }

        [Fact]
        public override async Task Scenario()
        {
            await base.Scenario();
        }
    }
}
