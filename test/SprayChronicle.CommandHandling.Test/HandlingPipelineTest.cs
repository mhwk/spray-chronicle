using System;
using System.Threading.Tasks;
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
    public class HandlingPipelineTest
    {
        private readonly IEventSourcingRepository<Basket> _baskets = Substitute.For<IEventSourcingRepository<Basket>>();

        [Fact]
        public async Task ItWontHandleUnsupportedCommand()
        {
            var router = new CommandRouter();
            var pipeline = new HandlingPipeline<HandleBasket, Basket>(_baskets, new HandleBasket());

            router.Subscribe(pipeline);
            
            await Should.ThrowAsync<UnhandledCommandException>(
                router.Route(new DoNotAcceptCommand())
            );
        }

        [Fact]
        public async Task ItDoesHandleSupportedCommand()
        {
            var router = new CommandRouter();
            var pipeline = new HandlingPipeline<HandleBasket, Basket>(_baskets, new HandleBasket());

            router.Subscribe(pipeline);
            
            await Should.ThrowAsync<UnhandledCommandException>(
                router.Route(new DoNotAcceptCommand())
            );
            
            await _baskets
                .Received()
                .Save<Basket>(Arg.Any<Basket>());
        }
        
        private class DoNotAcceptCommand
        {}
    }
}
