using System;
using SprayChronicle.Testing;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Contracts.Commands;
using SprayChronicle.Example.Contracts.Events;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class ItCanNotAddProductToCheckedOutBasket : EventSourcedTestCase<BasketHandler,Basket>
    {
        protected override object[] Given()
        {
            return new object[] {
                new BasketPickedUp("basketId"),
                new CheckOutBasket("basketId", "orderId")
            };
        }

        protected override object When()
        {
            return new AddProductToBasket("basketId", "productId");
        }

        protected override Type ExpectException()
        {
            return typeof(UnhandledDomainMessageException);
        }
    }
}
