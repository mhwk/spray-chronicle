using System;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.Example.Application;

namespace SprayChronicle.Example.Domain.Effect
{
    public class OrderReceptor : IHandleEvent<BasketCheckedOut>
    {
        readonly LoggingDispatcher _dispatcher;

        public OrderReceptor(LoggingDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void On(BasketCheckedOut message, DateTime epoch)
        {
            Console.WriteLine("-------------------_---_---_-_-_-_-_--_---_--_-");
            _dispatcher.Dispatch(new GenerateOrder(
                message.OrderId,
                message.ProductIds
            ));
        }
    }
}
