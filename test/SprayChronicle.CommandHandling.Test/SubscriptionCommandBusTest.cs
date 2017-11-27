using System;
using FluentAssertions;
using Moq;
using Xunit;

namespace SprayChronicle.CommandHandling.Test
{
    public class SubscriptionCommandBusTest
    {
        private readonly Mock<IHandleCommand> _commandHandler = new Mock<IHandleCommand>();

        [Fact]
        public void ItFailsIfNoSubscriptions()
        {
            var commandBus = new SubscriptionDispatcher();
            Action a = () => commandBus.Dispatch(new Command());
            a.ShouldThrow<UnhandledCommandException>();
        }

        [Fact]
        public void ItFailsIfNotAccepted()
        {
            _commandHandler.Setup(commandHandler => commandHandler.Handles(It.IsAny<object>())).Returns(false);

            var commandBus = new SubscriptionDispatcher();
            commandBus.Subscribe(_commandHandler.Object);

            Action a = () => commandBus.Dispatch(new Command());
            a.ShouldThrow<UnhandledCommandException>();
        }

        [Fact]
        public void ItDispatchesTohandler()
        {
            _commandHandler.Setup(commandHandler => commandHandler.Handles(It.IsAny<object>())).Returns(true);

            var commandBus = new SubscriptionDispatcher();
            commandBus.Subscribe(_commandHandler.Object);

            commandBus.Dispatch(new Command());
        }

        private class Command
        {}
    }
}