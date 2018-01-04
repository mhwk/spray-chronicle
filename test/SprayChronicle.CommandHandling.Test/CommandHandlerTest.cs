using System;
using Shouldly;
using NSubstitute;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Domain.Model;
using Xunit;

namespace SprayChronicle.CommandHandling.Test
{
    public class CommandHandlerTest
    {
        private readonly IEventSourcingRepository<Basket> _repository = Substitute.For<IEventSourcingRepository<Basket>>();

        [Fact]
        public void ItWontAcceptCommand()
        {
            new HandleBasket(_repository)
                .Handles(new DoNotAcceptCommand())
                .ShouldBeFalse();
        }

        [Fact]
        public void ItDoesAcceptCommand()
        {
            new HandleBasket(_repository)
                .Handles(new PickUpBasket("foo"))
                .ShouldBeTrue();
        }

        [Fact]
        public void ItFailsOnUnsupportedCommand()
        {
            Should.Throw<UnhandledCommandException>(
                () => new HandleBasket(_repository)
                    .Handle(new DoNotAcceptCommand())
            );
        }

        [Fact]
        public void ItAcceptsSupportedCommand()
        {
            new HandleBasket(_repository)
                .Handle(new PickUpBasket("foo"));
            
            _repository
                .Received()
                .Start(Arg.Any<Func<Basket>>());
        }
        
        private class DoNotAcceptCommand
        {}
    }
}
