using Shouldly;
using Xunit;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.MessageHandling.Test
{
    public class OverloadHandlingStrategyTest
    {
        private readonly OverloadHandlingStrategy<Basket> _strategy = new OverloadHandlingStrategy<Basket>(new ContextTypeLocator<Basket>());

        [Fact]
        public void ItAcceptsInitialMessage()
        {
            _strategy
                .AcceptsMessage(null, new BasketPickedUp("basketId").ToMessage())
                .ShouldBeTrue();
        }

        [Fact]
        public void ItDoesNotAcceptSecondMessage()
        {
            _strategy
                .AcceptsMessage(null, new ProductAddedToBasket("basketId", "productId").ToMessage())
                .ShouldBeFalse();
        }

        [Fact]
        public void ItDoesNotAcceptUnknownMessage()
        {
            _strategy
                .AcceptsMessage(null, new UnknownMessage().ToMessage())
                .ShouldBeFalse();
        }

        [Fact]
        public void ItReturnsAcceptedMethodResult()
        {
            _strategy
                .ProcessMessage(null, new BasketPickedUp("basketId").ToMessage())
                .ShouldBeAssignableTo<PickedUpBasket>();
        }

        [Fact]
        public void ItThrowsUnhandledMessageException()
        {
            Should.Throw<UnhandledMessageException>(() => _strategy.ProcessMessage(null, new {}.ToMessage()));
        }

        [Fact]
        public void ItThrowsUnexpectedStateExceptionExceptionForNewInstance()
        {
            Should.Throw<UnexpectedStateException>(() => _strategy.ProcessMessage(null, new ProductAddedToBasket("basketId", "productId").ToMessage()));
        }

        [Fact]
        public void ItThrowsUnexpectedStateExceptionExceptionForExistingInstance()
        {
            var basket = _strategy.ProcessMessage(null, new BasketPickedUp("basketId").ToMessage());
               
            Should.Throw<UnexpectedStateException>(
                () => _strategy.ProcessMessage(basket, new BasketPickedUp("basketId").ToMessage())
            );
        }

        [Fact]
        public void ItProcessesSecondMessage()
        {
            var basket = _strategy.ProcessMessage(null, new BasketPickedUp("basketId").ToMessage());
            
            _strategy
                .ProcessMessage(basket, new BasketCheckedOut("basketId", "orderId", new string[] {}).ToMessage())
                .ShouldBeAssignableTo<CheckedOutBasket>();
        }

        private class UnknownMessage
        {}
    }
}
