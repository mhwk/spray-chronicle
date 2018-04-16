using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Xunit;

namespace SprayChronicle.CommandHandling.Test
{
    public class SubscriptionCommandDispatcherTest
    {
        private readonly IHandleCommands _commandHandler = Substitute.For<IHandleCommands>();

        [Fact]
        public async Task ItFailsIfNoSubscriptions()
        {
            await Should.ThrowAsync<UnhandledCommandException>(
                async () => await new CommandRouter()
                    .Route(new object())
            );
        }

        [Fact]
        public async Task ItFailsIfNotAccepted()
        {
            _commandHandler.Handles(Arg.Any<object>()).Returns(false);

            await Should.ThrowAsync<UnhandledCommandException>(
                async () => await new CommandRouter()
                    .Subscribe(_commandHandler)
                    .Route(new object())
            );
        }

        [Fact]
        public async Task ItDispatchesToHandler()
        {
            _commandHandler.Handles(Arg.Any<object>()).Returns(true);

            await new CommandRouter()
                .Subscribe(_commandHandler)
                .Route(new object());
        }
    }
}