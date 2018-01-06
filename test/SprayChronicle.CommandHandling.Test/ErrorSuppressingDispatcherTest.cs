using System;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace SprayChronicle.CommandHandling.Test
{
    public class ErrorSuppressingDispatcherTest
    {
        private readonly IDispatchCommands _child = Substitute.For<IDispatchCommands>();
        
        [Fact]
        public async Task ItSuppressesErrors()
        {
            var command = new object();
            _child.Dispatch(Arg.Is(command)).Throws(new Exception("Whoops"));
            await new ErrorSuppressingDispatcher(_child).Dispatch(command);
        }
    }
}
