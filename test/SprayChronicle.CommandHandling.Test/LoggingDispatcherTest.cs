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
        private readonly ILogger<IDispatchCommands> _logger = Substitute.For<ILogger<IDispatchCommands>>();
        
        private readonly IMeasure _measure = Substitute.For<IMeasure>();
        
        private readonly IDispatchCommands _child = Substitute.For<IDispatchCommands>();

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
            
            await new LoggingDispatcher(_logger, _measure, _child).Dispatch(command);

            _logger
                .Received()
                .LogDebug("{0}: Dispatching...", command.GetType());
            _logger
                .Received()
                .LogInformation("{0}: {1}", command.GetType(), _measure);
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
                .Dispatch(Arg.Any<object>())
                .Throws(error);
            
            await Should.ThrowAsync<UnhandledCommandException>(() => new LoggingDispatcher(_logger, _measure, _child).Dispatch(command));

            _logger
                .Received()
                .LogDebug("{0}: Dispatching...", command.GetType());
            _logger
                .Received()
                .LogWarning(error, "{0}: Not handled", command.GetType());
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
                .Dispatch(Arg.Any<object>())
                .Throws(error);

            await Should.ThrowAsync<Exception>(() => new LoggingDispatcher(_logger, _measure, _child).Dispatch(command));

            _logger
                .Received()
                .LogDebug("{0}: Dispatching...", command.GetType());
            _logger
                .Received()
                .LogDebug(error, "{0}: Domain exception", command.GetType());
        }
    }
}
