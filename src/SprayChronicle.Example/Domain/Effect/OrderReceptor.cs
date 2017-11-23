using System;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.Example.Application;

namespace SprayChronicle.Example.Domain.Effect
{
    public class OrderReceptor : IHandleEvent<BasketCheckedOut>
    {
        private readonly LoggingDispatcher _dispatcher;

        public OrderReceptor(LoggingDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void On(BasketCheckedOut message, DateTime epoch)
        {
            _dispatcher.Dispatch(new GenerateOrder(
                message.OrderId,
                message.ProductIds
            ));
        }
    }
}
