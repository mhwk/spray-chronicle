using SprayChronicle.Testing;
using SprayChronicle.Example.Contracts.Commands;
using SprayChronicle.Example.Contracts.Events;
using SprayChronicle.Example.Coordination;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class ItCanPickUpABasket : EventSourcedTestCase<BasketHandler,Basket>
    {
        protected override object When()
        {
            return new PickUpBasket("basketId");
        }

        protected override object[] Expect()
        {
            return new object[] {
                new BasketPickedUp("basketId")
            };
        }
    }
}
