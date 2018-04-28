using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Autofac;
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
            var dispatcher = new RouterCommandDispatcher(router);
            var pipeline = new HandlingPipeline<HandleBasket, Basket>(_baskets, new HandleBasket(), buffer);

            router.Subscribe(pipeline);
            
            Should.Throw<UnroutableMessageException>(
                dispatcher.Dispatch(new DoNotAcceptCommand())
            );
        }

        [Fact]
        public async Task ItDoesHandleFirstCommand()
        {
            var buffer = new BufferBlock<CommandEnvelope>();
            var router = new CommandRouter();
            var dispatcher = new RouterCommandDispatcher(router);
            var pipeline = new HandlingPipeline<HandleBasket, Basket>(_baskets, new HandleBasket(), buffer);
            var task = pipeline.Start();
            
            router.Subscribe(pipeline);
            await dispatcher.Dispatch(new PickUpBasket("basketId"));
            buffer.Complete();
            
            await _baskets
                .Received()
                .Save<Basket>(Arg.Any<Basket>(), Arg.Any<IEnvelope>());
        }

        [Fact]
        public async Task ItDoesHandleSecondCommand()
        {
            var buffer = new BufferBlock<CommandEnvelope>();
            var router = new CommandRouter();
            var dispatcher = new RouterCommandDispatcher(router);
            var pipeline = new HandlingPipeline<HandleBasket, Basket>(_baskets, new HandleBasket(), buffer);
            var task = pipeline.Start();

            _baskets.Load<Basket>(Arg.Any<string>(), Arg.Any<string>()).Returns(new PickedUpBasket("basketId"));
            
            router.Subscribe(pipeline);
            await dispatcher.Dispatch(new PickUpBasket("basketId"));
            await dispatcher.Dispatch(new AddProductToBasket("basketId", "productId"));
            buffer.Complete();
            
            await _baskets
                .Received()
                .Save<Basket>(Arg.Any<PickedUpBasket>(), Arg.Any<IEnvelope>());
            await _baskets
                .Received()
                .Save<Basket>(Arg.Any<PickedUpBasket>(), Arg.Any<IEnvelope>());
        }

        [Fact]
        public async Task ItDoesHandleThirdCommand()
        {
            var buffer = new BufferBlock<CommandEnvelope>();
            var router = new CommandRouter();
            var dispatcher = new RouterCommandDispatcher(router);
            var pipeline = new HandlingPipeline<HandleBasket, Basket>(_baskets, new HandleBasket(), buffer);
            var task = pipeline.Start();
            
            _baskets.Load<Basket>(Arg.Any<string>(), Arg.Any<string>()).Returns(new PickedUpBasket("basketId"));

            router.Subscribe(pipeline);
            await dispatcher.Dispatch(new PickUpBasket("basketId"));
            await dispatcher.Dispatch(new AddProductToBasket("basketId", "productId"));
            await dispatcher.Dispatch(new CheckOutBasket("basketId", "orderId"));
            buffer.Complete();
            
            await _baskets
                .Received()
                .Save<Basket>(Arg.Any<PickedUpBasket>(), Arg.Any<IEnvelope>());
            await _baskets
                .Received()
                .Save<Basket>(Arg.Any<PickedUpBasket>(), Arg.Any<IEnvelope>());
            await _baskets
                .Received()
                .Save<Basket>(Arg.Any<CheckedOutBasket>(), Arg.Any<IEnvelope>());
        }
        
        private class DoNotAcceptCommand
        {}
    }
}
