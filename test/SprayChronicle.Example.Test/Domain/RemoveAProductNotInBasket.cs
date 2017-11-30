using System;
using SprayChronicle.Testing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class RemoveAProductNotInBasket : EventSourcedTestCase<Module,Basket>
    {
        protected override object[] Given()
        {
            return new object[] {
                new BasketPickedUp("basketId")
            };
        }

        protected override Basket When(Basket basket)
        {
            return (basket as PickedUpBasket).RemoveProduct(new ProductId("productId"));
        }

        protected override Type ExpectException()
        {
            return typeof(ProductNotInBasketException);
        }
    }
}
