using System;
using Shouldly;
using NSubstitute;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.MessageHandling;
using Xunit;

namespace SprayChronicle.CommandHandling.Test
{
    public class CommandHandlerTest
    {
        private readonly IEventSourcingRepository<Basket> _baskets = Substitute.For<IEventSourcingRepository<Basket>>();
        
        private readonly IEventSourcingRepository<Order> _orders = Substitute.For<IEventSourcingRepository<Order>>();

        [Fact]
        public void ItWontAcceptCommand()
        {
            new HandleBasket(_baskets)
                .Handles(new DoNotAcceptCommand())
                .ShouldBeFalse();
        }

        [Fact]
        public void ItDoesAcceptCommand()
        {
            new HandleBasket(_baskets)
                .Handles(new PickUpBasket("foo"))
                .ShouldBeTrue();
        }

        [Fact]
        public void ItWontHandleUnsupportedCommand()
        {
            Should.Throw<UnhandledCommandException>(
                () => new HandleBasket(_baskets)
                    .Handle(new DoNotAcceptCommand())
            );
        }

        [Fact]
        public void ItDoesHandleSupportedCommand()
        {
            new HandleBasket(_baskets)
                .Handle(new PickUpBasket("foo"));
            
            _baskets
                .Received()
                .Start(Arg.Any<Func<Basket>>());
        }

        [Fact]
        public void ItWontAcceptMessage()
        {
            var message = new object();
            var epoch = new DateTime();
            
            new HandleOrder(_orders)
                .Processes(message, epoch)
                .ShouldBeFalse();
        }
        
        [Fact]
        public void ItDoesAcceptMessage()
        {
            var message = new BasketCheckedOut("basketId", "orderId", new [] { "productId" });
            var epoch = new DateTime();
            
            new HandleOrder(_orders)
                .Processes(message, epoch)
                .ShouldBeTrue();
        }

        [Fact]
        public void ItWontProcessUnsupportedMessage()
        {
            var message = new object();
            var epoch = new DateTime();
            
            Should.Throw<UnhandledMessageException>(() => new HandleOrder(_orders).Process(message, epoch));

            _orders.DidNotReceive().Start(Arg.Any<Func<Order>>());
        }
        
        [Fact]
        public void ItDoesProcessSupportedMessage()
        {
            var message = new BasketCheckedOut("basketId", "orderId", new [] { "productId" });
            var epoch = new DateTime();
            
            new HandleOrder(_orders).Process(message, epoch);

            _orders.Received().Start(Arg.Any<Func<Order>>());
        }


        private class DoNotAcceptCommand
        {}
    }
}
