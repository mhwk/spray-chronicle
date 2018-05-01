using System.Threading.Tasks;
using SprayChronicle.Testing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class PickUpABasket : EventSourcedTestCase<Module,Basket>
    {
        protected override Task<Basket> When(Basket basket)
        {
            return Basket.PickUp(new BasketId("basketId"));
        }

        protected override void Then(IValidate validator)
        {
            validator.Expect(new BasketPickedUp("basketId"));
        }
    }
}
