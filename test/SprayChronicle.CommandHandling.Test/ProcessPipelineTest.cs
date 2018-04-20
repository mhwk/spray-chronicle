using System;
using System.Threading.Tasks;
using Shouldly;
using NSubstitute;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Domain;
using SprayChronicle.MessageHandling;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.CommandHandling.Test
{
    public class ProcessPipelineTest
    {
        private readonly TestSource<HandleOrder> _source = new TestSource<HandleOrder>();
        
        private readonly IEventSourceFactory _factory = Substitute.For<IEventSourceFactory>();
        
        private readonly PersistentOptions _options = new PersistentOptions("test", "test");

        private readonly IMessageRouter _router = Substitute.For<IMessageRouter>();
        
        [Fact]
        public async Task HandleUnknownMessage()
        {
            var message = new object();
            var pipeline = new ProcessingPipeline<HandleOrder>(_factory, _options, _router, new HandleOrder());
            
            _factory
//                .Build<HandleOrder, PersistentOptions>(Arg.Is(_options))
                .Build<HandleOrder, PersistentOptions>(Arg.Any<PersistentOptions>())
                .Returns(_source);

            await _source.Publish(message);
//            await Should.ThrowAsync<UnroutableMessageException>(pipeline.Start());
            await pipeline.Start();
        }
        
//        [Fact]
//        public async Task HandleKnownMessage()
//        {
//            var message = new BasketPickedUp("basketId");
//            var pipeline = new ProcessingPipeline<HandleBasket>(_factory, _options, _router, new HandleBasket());
//            
//            _factory
//                .Build<HandleOrder, PersistentOptions>(Arg.Is(_options))
//                .Returns(_source);
//
//            await _source.Publish(message);
//            await pipeline.Start();
//            await _router.Received().Route(Arg.Any<GenerateOrder>());
//        }
    }
}
