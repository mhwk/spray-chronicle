using SprayChronicle.Example.Domain;
using SprayChronicle.Example;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Testing.Test
{
    public class EventSourcedTestCaseTest : EventSourcedTestCase<Module,Basket>
    {
        protected override Basket Given(Basket basket)
        {
            return Basket.PickUp(new BasketId("basketId"));
        }

        protected override Basket When(Basket basket)
        {
            return (basket as PickedUpBasket)?.AddProduct(new ProductId("productId"));
        }

        protected override void Then(IValidate validator)
        {
            validator.Expect(
                new ProductAddedToBasket("basketId", "productId")
            );
        }
    }
}
