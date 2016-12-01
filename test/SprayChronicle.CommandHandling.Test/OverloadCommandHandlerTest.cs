using System;
using Xunit;
using Moq;
using FluentAssertions;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Contracts.Commands;

namespace SprayChronicle.Test.CommandHandling
{
    public class OverloadCommandHandlerTest
    {
        public Mock<IObjectRepository<Basket>> Repository = new Mock<IObjectRepository<Basket>>();

        [Fact]
        public void ItWontAcceptCommand()
        {
            new BasketHandler(Repository.Object).Handles(new DoNotAcceptCommand()).Should().BeFalse();
        }

        [Fact]
        public void IDoesAcceptCommand()
        {
            new BasketHandler(Repository.Object).Handles(new PickUpBasket("foo")).Should().BeTrue();
        }

        [Fact]
        public void ItFailsOnUnsupportedCommand()
        {
            Action a = () => new BasketHandler(Repository.Object).Handle(new DoNotAcceptCommand());
            a.ShouldThrow<UnhandledCommandException>();
        }

        [Fact]
        public void ItAcceptsSupportedCommand()
        {
            new BasketHandler(Repository.Object).Handle(new PickUpBasket("foo"));
            Repository.Verify(repository => repository.Save(It.IsAny<Basket>()));
        }

        public class DoNotAcceptCommand
        {}
    }
}
