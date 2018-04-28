using System;
using System.Threading.Tasks;
using Shouldly;
using SprayChronicle.Example.Application;
using Xunit;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.MessageHandling.Test
{
    public class OverloadMessagingStrategyTest
    {
        [Fact]
        public void ItAcceptsMessage()
        {
            new OverloadMailStrategy<Basket>()
                .Resolves(new BasketPickedUp("basketId"))
                .ShouldBeTrue();
        }

        [Fact]
        public void ItAcceptsMessageOfOverload()
        {
            new OverloadMailStrategy<Basket>()
                .Resolves(new ProductAddedToBasket("basketId", "productId"))
                .ShouldBeTrue();
        }

        [Fact]
        public void ItDoesNotAcceptUnknownMessage()
        {
            new OverloadMailStrategy<Basket>()
                .Resolves(new UnknownMessage())
                .ShouldBeFalse();
        }
        
//        [Fact]
//        public void ItAcceptsFactoryMessage()
//        {
//            _strategy
//                .Resolves(null, new BasketPickedUp("basketId"))
//                .ShouldBeTrue();
//        }
//        
//        [Fact]
//        public void ItDoesNotAcceptFactoryMessageForInstance()
//        {
//            _strategy
//                .Resolves(new PickedUpBasket("basketId"), new BasketPickedUp("basketId"))
//                .ShouldBeFalse();
//        }
//
//        [Fact]
//        public void ItDoesNotAcceptInstanceMessageForNull()
//        {
//            _strategy
//                .Resolves(null, new ProductAddedToBasket("basketId", "productId"))
//                .ShouldBeFalse();
//        }

        [Fact]
        public void TellUnhandledMessage()
        {
            Should.Throw<UnsupportedMessageException>(
                () => new OverloadMailStrategy<Basket>().Tell(null, new UnknownMessage(), DateTime.Now)
            );
        }

        [Fact]
        public void TellInstanceMethodToNull()
        {
            Should.Throw<UnexpectedStateException>(
                () => new OverloadMailStrategy<Basket>().Tell(null, new ProductAddedToBasket("basketId", "productId"), DateTime.Now)
            );
        }

        [Fact]
        public void TellFactoryMethodToInstance()
        {
            Should.Throw<UnexpectedStateException>(
                () => new OverloadMailStrategy<Basket>().Tell(new PickedUpBasket("basketId"), new BasketPickedUp("basketId"), DateTime.Now)
            );
        }

        [Fact]
        public async Task AskMessageResult()
        {
            var result = await new OverloadMailStrategy<Basket>().Ask<Basket>(null, new BasketPickedUp("basketId"), DateTime.Now);
            result.ShouldBeAssignableTo<PickedUpBasket>();
        }

        [Fact]
        public async Task ThrowUnexpectedStateExceptionForExistingInstance()
        {
            var basket = await new OverloadMailStrategy<Basket>().Ask<Basket>(null, new BasketPickedUp("basketId"), DateTime.Now);
               
            Should.Throw<UnexpectedStateException>(
                () => new OverloadMailStrategy<Basket>().Ask<Basket>(basket, new BasketPickedUp("basketId"), DateTime.Now)
            );
        }

        [Fact]
        public async Task ProcessSecondMessage()
        {
            var basket = await new OverloadMailStrategy<Basket>().Ask<Basket>(null, new BasketPickedUp("basketId"), DateTime.Now);
            basket = await new OverloadMailStrategy<Basket>().Ask<Basket>(basket, new BasketCheckedOut("basketId", "orderId", new string[0]), DateTime.Now);
            
            basket.ShouldBeAssignableTo<CheckedOutBasket>();
        }

        private class UnknownMessage
        {}
    }
}
