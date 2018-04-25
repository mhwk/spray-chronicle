using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Shouldly;
using NSubstitute;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.MessageHandling;
using Xunit;

namespace SprayChronicle.CommandHandling.Test
{
    public class HandlingPipelineTest
    {
        private readonly IEventSourcingRepository<Basket> _baskets = Substitute.For<IEventSourcingRepository<Basket>>();

        [Fact]
        public void ItWontHandleUnsupportedCommand()
        {
            var buffer = new BufferBlock<CommandEnvelope>();
            var router = new CommandRouter();
            var pipeline = new HandlingPipeline<HandleBasket, Basket>(_baskets, new HandleBasket(), buffer);

            router.Subscribe(pipeline);
            
            Should.Throw<UnroutableMessageException>(
                router.Route(new DoNotAcceptCommand())
            );
        }

        [Fact]
        public async Task ItDoesHandleSupportedCommand()
        {
            var buffer = new BufferBlock<CommandEnvelope>();
            var router = new CommandRouter();
            var pipeline = new HandlingPipeline<HandleBasket, Basket>(_baskets, new HandleBasket(), buffer);
            var task = pipeline.Start();
            
            router.Subscribe(pipeline);
            await router.Route(new PickUpBasket("basketId"));
            buffer.Complete();
            
            await _baskets
                .Received()
                .Save<Basket>(Arg.Any<Basket>());
        }
        
        private class DoNotAcceptCommand
        {}
    }
}
