using SprayChronicle.Testing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class CheckOutAFilledBasket : EventSourcedTestCase<Module,Basket>
    {
        protected override Basket Given(Basket basket)
        {
            return Basket
                .PickUp(new BasketId("basketId"))
                .AddProduct(new ProductId("productId"));
        }

        protected override Basket When(Basket basket)
        {
            return (basket as PickedUpBasket)?.CheckOut(new OrderId("orderId"));
        }

        protected override object[] Expect()
        {
            return new object[] {
                new BasketCheckedOut("basketId", "orderId", new [] {"productId"}),
            };
        }
    }
}
