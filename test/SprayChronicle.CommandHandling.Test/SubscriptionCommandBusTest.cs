using System;
using Xunit;
using Moq;
using FluentAssertions;
using SprayChronicle.CommandHandling;

namespace SprayChronicle.Test.CommandHandling
{
    public class SubscriptionCommandBusTest
    {
        public Mock<IHandleCommand> CommandHandler = new Mock<IHandleCommand>();

        [Fact]
        public void ItFailsIfNoSubscriptions()
        {
            var commandBus = new SubscriptionCommandBus();
            Action a = () => commandBus.Dispatch(new Command());
            a.ShouldThrow<UnhandledCommandException>();
        }

        [Fact]
        public void ItFailsIfNotAccepted()
        {
            CommandHandler.Setup(commandHandler => commandHandler.Handles(It.IsAny<object>())).Returns(false);

            var commandBus = new SubscriptionCommandBus();
            commandBus.Subscribe(CommandHandler.Object);

            Action a = () => commandBus.Dispatch(new Command());
            a.ShouldThrow<UnhandledCommandException>();
        }

        [Fact]
        public void ItDispatchesTohandler()
        {
            CommandHandler.Setup(commandHandler => commandHandler.Handles(It.IsAny<object>())).Returns(true);

            var commandBus = new SubscriptionCommandBus();
            commandBus.Subscribe(CommandHandler.Object);

            commandBus.Dispatch(new Command());
        }

        public class Command
        {}
    }
}