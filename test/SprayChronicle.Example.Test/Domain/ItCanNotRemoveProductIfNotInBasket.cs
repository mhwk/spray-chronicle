using System;
using SprayChronicle.Testing;
using SprayChronicle.Example.Contracts.Commands;
using SprayChronicle.Example.Contracts.Events;
using SprayChronicle.Example.Coordination;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class ItCanNotRemoveProductifNotInBasket : EventSourcedTestCase<BasketHandler,Basket>
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
