using System;
using System.Collections.Immutable;
using FluentAssertions;
using Moq;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Domain.Model;
using Xunit;

namespace SprayChronicle.CommandHandling.Test
{
    public class OverloadCommandHandlerTest
    {
        public Mock<IEventSourcingRepository<Basket>> Repository = new Mock<IEventSourcingRepository<Basket>>();

        [Fact]
        public void ItWontAcceptCommand()
        {
            new BasketHandler(Repository.Object).Handles(new DoNotAcceptCommand()).Should().BeFalse();
        }

        [Fact]
        public void ItDoesAcceptCommand()
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
        
        [Fact]
        public void ItThrowsDomainException()
        {
            Repository.Setup(r => r.Load<PickedUpBasket>(It.IsAny<string>())).Returns(new PickedUpBasket(new BasketId("foo"), ImmutableList.Create<ProductId>()));
            Action a = () => new BasketHandler(Repository.Object).Handle(new RemoveProductFromBasket("foo", "bar"));
            a.ShouldThrow<ProductNotInBasketException>();
        }

        private class DoNotAcceptCommand
        {}
    }
}
