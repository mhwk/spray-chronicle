using System;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using SprayChronicle.Server;
using Xunit;

namespace SprayChronicle.CommandHandling.Test
{
    public class LoggingDispatcherTest
    {
        private readonly ILogger<ICommandRouter> _logger = Substitute.For<ILogger<ICommandRouter>>();
        
        private readonly IMeasure _measure = Substitute.For<IMeasure>();
        
        private readonly ICommandRouter _child = Substitute.For<ICommandRouter>();

        [Fact]
        public async Task HappyFlow()
        {
            var command = new object();
            
            _measure
                .Start()
                .Returns(_measure);
            _measure
                .Stop()
                .Returns(_measure);
            
            await new LoggingRouter(_logger, _measure, _child).Route(command);

            _logger
                .Received()
                .LogDebug("{0}: Dispatching...", "Object");
            _logger
                .Received()
                .LogInformation("{0}: {1}", "Object", _measure);
        }
        
        [Fact]
        public async Task UnhandledCommand()
        {
            var command = new object();
            var error = new UnhandledCommandException("Not handled");
            
            _measure
                .Start()
                .Returns(_measure);
            _measure
                .Stop()
                .Returns(_measure);
            _child
                .Route(Arg.Any<object>())
                .Throws(error);
            
            await Should.ThrowAsync<UnhandledCommandException>(() => new LoggingRouter(_logger, _measure, _child).Route(command));

            _logger
                .Received()
                .LogDebug("{0}: Dispatching...", "Object");
            _logger
                .Received()
                .LogWarning(error, "{0}: Not handled", "Object");
            _logger
                .Received()
                .LogInformation("{0}: {1}", "Object", _measure);
        }
        
        [Fact]
        public async Task DomainError()
        {
            var command = new object();
            var error = new Exception("Domain error");
            
            _measure
                .Start()
                .Returns(_measure);
            _measure
                .Stop()
                .Returns(_measure);
            _child
                .Route(Arg.Any<object>())
                .Throws(error);

            await Should.ThrowAsync<Exception>(() => new LoggingRouter(_logger, _measure, _child).Route(command));

            _logger
                .Received()
                .LogDebug("{0}: Dispatching...", "Object");
            _logger
                .Received()
                .LogDebug(error, "{0}: Domain exception", "Object");
            _logger
                .Received()
                .LogInformation("{0}: {1}", "Object", _measure);
        }
    }
}
