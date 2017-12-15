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
            new HandleBasket(Repository.Object).Handles(new DoNotAcceptCommand()).Should().BeFalse();
        }

        [Fact]
        public void ItDoesAcceptCommand()
        {
            new HandleBasket(Repository.Object).Handles(new PickUpBasket("foo")).Should().BeTrue();
        }

        [Fact]
        public void ItFailsOnUnsupportedCommand()
        {
            Action a = () => new HandleBasket(Repository.Object).Handle(new DoNotAcceptCommand());
            a.ShouldThrow<UnhandledCommandException>();
        }

        [Fact]
        public void ItAcceptsSupportedCommand()
        {
            new HandleBasket(Repository.Object).Handle(new PickUpBasket("foo"));
            Repository.Verify(repository => repository.Start(It.IsAny<Func<Basket>>()));
        }
        
//        [Fact]
//        public void ItThrowsDomainException()
//        {
//            Repository.Setup(r => r.Load<PickedUpBasket>(It.IsAny<string>())).Returns(new PickedUpBasket(new BasketId("foo"), ImmutableList.Create<ProductId>()));
//            Action a = () => new BasketCommandHandler(Repository.Object).Handle(new RemoveProductFromBasket("foo", "bar"));
//            a.ShouldThrow<ProductNotInBasketException>();
//        }

        private class DoNotAcceptCommand
        {}
    }
}
