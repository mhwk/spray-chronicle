using Xunit;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.MessageHandling.Test
{
    public class GivenOverloadHandlingStrategyForBasket
    {
        private readonly OverloadHandlingStrategy<Basket> _strategy = new OverloadHandlingStrategy<Basket>(new ContextTypeLocator<Basket>());

        [Fact]
        public void ItAcceptsInitialMessage()
        {
            Assert.True(_strategy.AcceptsMessage(null, new BasketPickedUp("basketId").ToMessage()));
        }

        [Fact]
        public void ItDoesNotAcceptSecondMessage()
        {
            Assert.False(_strategy.AcceptsMessage(null, new ProductAddedToBasket("basketId", "productId").ToMessage()));
        }

        [Fact]
        public void ItDoesNotAcceptUnknownMessage()
        {
            Assert.False(_strategy.AcceptsMessage(null, new UnknownMessage().ToMessage()));
        }

        [Fact]
        public void ItReturnsAcceptedMethodResult()
        {
            Assert.IsAssignableFrom<PickedUpBasket>(_strategy.ProcessMessage(null, new BasketPickedUp("basketId").ToMessage()));
        }

        [Fact]
        public void ItThrowsUnhandledMessageException()
        {
            Assert.Throws<UnhandledMessageException>(() => _strategy.ProcessMessage(null, new {}.ToMessage()));
        }

        [Fact]
        public void ItThrowsUnexpectedStateExceptionExceptionForNewInstance()
        {
            Assert.Throws<UnexpectedStateException>(() => _strategy.ProcessMessage(null, new ProductAddedToBasket("basketId", "productId").ToMessage()));
        }

        [Fact]
        public void ItThrowsUnexpectedStateExceptionExceptionForExistingInstance()
        {
            var basket = _strategy.ProcessMessage(null, new BasketPickedUp("basketId").ToMessage());
            Assert.Throws<UnexpectedStateException>(() => _strategy.ProcessMessage(basket, new BasketPickedUp("basketId").ToMessage()));
        }

        [Fact]
        public void ItProcessesSecondMessage()
        {
            var basket = _strategy.ProcessMessage(null, new BasketPickedUp("basketId").ToMessage());
            Assert.IsAssignableFrom<CheckedOutBasket>(_strategy.ProcessMessage(basket, new BasketCheckedOut("basketId", "orderId", new string[] {}).ToMessage()));
        }

        private class UnknownMessage
        {}
    }
}
