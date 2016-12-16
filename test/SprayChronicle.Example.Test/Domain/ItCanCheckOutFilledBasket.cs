using SprayChronicle.Testing;
using SprayChronicle.Example.Contracts.Commands;
using SprayChronicle.Example.Contracts.Events;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class ItCanCheckOutFilledBasket : EventSourcedTestCase<ExampleCoordinationModule>
    {
        protected override object[] Given()
        {
            return new object[] {
                new BasketPickedUp("basketId"),
                new ProductAddedToBasket("basketId", "productId")
            };
        }

        protected override object When()
        {
            return new CheckOutBasket("basketId", "orderId");
        }

        protected override object[] Expect()
        {
            return new object[] {
                new BasketCheckedOut("basketId", "orderId")
            };
        }
    }
}
