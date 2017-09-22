using System;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Contracts.Commands;
using SprayChronicle.Example.Contracts.Events;
using SprayChronicle.Testing;

namespace SprayChronicle.Example.Test.Domain
{
    public class ItCanNotPickUpBasketWithoutId : EventSourcedTestCase<ExampleCoordinationModule>
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
