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
        private readonly OverloadMessagingStrategy<Basket> _strategy = new OverloadMessagingStrategy<Basket>();

        [Fact]
        public void ItAcceptsMessage()
        {
            _strategy
                .Resolves(new BasketPickedUp("basketId"))
                .ShouldBeTrue();
        }

        [Fact]
        public void ItAcceptsMessageOfOverload()
        {
            _strategy
                .Resolves(new ProductAddedToBasket("basketId", "productId"))
                .ShouldBeTrue();
        }

        [Fact]
        public void ItDoesNotAcceptUnknownMessage()
        {
            _strategy
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
            Should.Throw<UnhandledMessageException>(() => _strategy.Tell(null, new UnknownMessage()));
        }

        [Fact]
        public void TellInstanceMethodToNull()
        {
            Should.Throw<UnexpectedStateException>(() => _strategy.Tell(null, new ProductAddedToBasket("basketId", "productId")));
        }

        [Fact]
        public void TellFactoryMethodToInstance()
        {
            Should.Throw<UnexpectedStateException>(() => _strategy.Tell(new PickedUpBasket("basketId"), new BasketPickedUp("basketId")));
        }

        [Fact]
        public async Task AskMessageResult()
        {
            var result = await _strategy.Ask<Basket>(null, new BasketPickedUp("basketId"));
            result.ShouldBeAssignableTo<PickedUpBasket>();
        }

        [Fact]
        public async Task ThrowUnexpectedStateExceptionForExistingInstance()
        {
            var basket = await _strategy.Ask<Basket>(null, new BasketPickedUp("basketId"));
               
            Should.Throw<UnexpectedStateException>(() => _strategy.Ask<Basket>(basket, new BasketPickedUp("basketId")));
        }

        [Fact]
        public async Task ProcessSecondMessage()
        {
            var basket = await _strategy.Ask<Basket>(null, new BasketPickedUp("basketId"));
            basket = await _strategy.Ask<Basket>(basket, new BasketCheckedOut("basketId", "orderId", new string[0]));
            
            basket.ShouldBeAssignableTo<CheckedOutBasket>();
        }

        private class UnknownMessage
        {}
    }
}
