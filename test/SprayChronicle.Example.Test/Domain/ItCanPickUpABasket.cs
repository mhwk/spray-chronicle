using SprayChronicle.Testing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class ItCanPickUpABasket : EventSourcedTestCase<ExampleModule>
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
