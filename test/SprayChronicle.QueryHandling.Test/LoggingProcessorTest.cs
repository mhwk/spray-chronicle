using System;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using SprayChronicle.Server;
using Xunit;

namespace SprayChronicle.QueryHandling.Test
{
    public class LoggingProcessorTest
    {
        private readonly ILogger<IQueryRouter> _logger = Substitute.For<ILogger<IQueryRouter>>();
        
        private readonly IMeasure _measure = Substitute.For<IMeasure>();
        
        private readonly IQueryRouter _child = Substitute.For<IQueryRouter>();

        [Fact]
        public async Task HappyFlow()
        {
            var command = new object();
            var result = new object();

            _measure
                .Start()
                .Returns(_measure);
            _measure
                .Stop()
                .Returns(_measure);

            _child.Route(Arg.Is(command)).Returns(result);
            
            (await new LoggingRouter(_logger, _measure, _child)
                .Route(command))
                .ShouldBe(result);

            _logger
                .Received()
                .LogDebug("{0}: Processing...", "Object");
            _logger
                .Received()
                .LogInformation("{0}: {1}", "Object", _measure);
        }
        
        [Fact]
        public async Task ProjectionError()
        {
            var command = new object();
            var error = new Exception();

            _measure
                .Start()
                .Returns(_measure);
            _measure
                .Stop()
                .Returns(_measure);

            _child.Route(Arg.Is(command)).Throws(error);
            
            await Should.ThrowAsync<Exception>(
                async () => await new LoggingRouter(_logger, _measure, _child)
                    .Route(command)
            );

            _logger
                .Received()
                .LogDebug("{0}: Processing...", "Object");
            _logger
                .Received()
                .LogError(error, "{0}: Projection failure", "Object");
            _logger
                .Received()
                .LogInformation("{0}: {1}", "Object", _measure);
        }
    }
}
