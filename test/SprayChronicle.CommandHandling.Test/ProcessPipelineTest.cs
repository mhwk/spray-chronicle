using System;
using System.Threading.Tasks;
using Shouldly;
using NSubstitute;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Domain;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.CommandHandling.Test
{
    public class ProcessPipelineTest
    {
        private readonly TestSource<HandleOrder> _source = new TestSource<HandleOrder>();
        
        private readonly IEventSourceFactory _factory = Substitute.For<IEventSourceFactory>();
        
        private readonly CatchUpOptions _options = new CatchUpOptions("test");

        private readonly IMailRouter _router = Substitute.For<IMailRouter>();

        private readonly ILogger<HandleOrder> _logger = Substitute.For<ILogger<HandleOrder>>();
        
        [Fact]
        public async Task HandleUnknownMessage()
        {
            var message = new object();
            var pipeline = new ProcessingPipeline<HandleOrder>(
                _logger,
                _factory,
                _options,
                _router,
                new HandleOrder()
            );
            
            _factory
                .Build<HandleOrder, CatchUpOptions>(Arg.Is(_options))
                .Returns(_source);

            await _source.Publish(message);
            _source.Complete();

            await pipeline.Start();
            
            _logger.Received().LogWarning(Arg.Any<UnsupportedMessageException>());
        }
        
        [Fact]
        public async Task HandleKnownMessage()
        {
            var message = new BasketCheckedOut("basketId", "orderId", new string[0]);
            var pipeline = new ProcessingPipeline<HandleOrder>(
                _logger,
                _factory,
                _options,
                _router,
                new HandleOrder()
            );
            
            _factory
                .Build<HandleOrder, CatchUpOptions>(Arg.Is(_options))
                .Returns(_source);

            await _source.Publish(message);
            _source.Complete();
            
            await pipeline.Start();
            
            _logger.DidNotReceive().LogDebug(Arg.Any<Exception>());
            await _router.Received().Route(Arg.Any<IEnvelope>());
        }
    }
}
