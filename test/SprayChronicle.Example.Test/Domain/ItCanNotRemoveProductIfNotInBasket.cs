using System;
using SprayChronicle.Testing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class ItCanNotRemoveProductifNotInBasket : EventSourcedTestCase<ExampleModule>
    {
        protected override object[] Given()
        {
            return new object[] {
                new BasketPickedUp("basketId")
            };
        }

        protected override object When()
        {
            return new RemoveProductFromBasket("basketId", "productId");
        }

        protected override Type ExpectException()
        {
            return typeof(ProductNotInBasketException);
        }
    }
}
