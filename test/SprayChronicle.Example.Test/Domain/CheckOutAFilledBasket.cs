using System.Threading.Tasks;
using SprayChronicle.Testing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class CheckOutAFilledBasket : EventSourcedTestCase<Module,Basket>
    {
        protected override async Task<Basket> Given()
        {
            var basket = await Basket.PickUp(new BasketId("basketId"));
            basket = await ((PickedUpBasket) basket).AddProduct(new ProductId("productId"));
            return basket;
        }

        protected override Task<Basket> When(Basket basket)
        {
            return (basket as PickedUpBasket)?.CheckOut(new OrderId("orderId"));
        }

        protected override void Then(IValidate validator)
        {
            validator.Expect(new BasketCheckedOut("basketId", "orderId", new [] {"productId"}));
        }
    }
}
