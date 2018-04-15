using System;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace SprayChronicle.CommandHandling.Test
{
    public class ErrorSuppressingDispatcherTest
    {
        private readonly ICommandRouter _child = Substitute.For<ICommandRouter>();
        
        [Fact]
        public async Task ItSuppressesErrors()
        {
            var command = new object();
            _child.Route(Arg.Is(command)).Throws(new Exception("Whoops"));
            await new ErrorSuppressingRouter(_child).Route(command);
        }
    }
}
