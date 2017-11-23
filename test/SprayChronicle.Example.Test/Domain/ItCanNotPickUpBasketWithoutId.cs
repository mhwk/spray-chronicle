using System;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Domain;
using SprayChronicle.Testing;

namespace SprayChronicle.Example.Test.Domain
{
    public class ItCanNotPickUpBasketWithoutId : EventSourcedTestCase<Module>
    {
        protected override object When()
        {
            return new PickUpBasket("");
        }

        protected override Type ExpectException()
        {
            return typeof(UnknownStreamException);
        }
    }
}
