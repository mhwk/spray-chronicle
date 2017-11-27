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
            Assert.True(_strategy.AcceptsMessage(null, new InstanceMessage(new BasketPickedUp("basketId"))));
        }

        [Fact]
        public void ItDoesNotAcceptSecondMessage()
        {
            Assert.False(_strategy.AcceptsMessage(null, new InstanceMessage(new ProductAddedToBasket("basketId", "productId"))));
        }

        [Fact]
        public void ItDoesNotAcceptUnknownMessage()
        {
            Assert.False(_strategy.AcceptsMessage(null, new InstanceMessage(new UnknownMessage())));
        }

        [Fact]
        public void ItReturnsAcceptedMethodResult()
        {
            Assert.IsAssignableFrom<PickedUpBasket>(_strategy.ProcessMessage(null, new InstanceMessage(new BasketPickedUp("basketId"))));
        }

        [Fact]
        public void ItThrowsUnhandledMessageException()
        {
            Assert.Throws<UnhandledMessageException>(() => _strategy.ProcessMessage(null, new InstanceMessage(new {})));
        }

        [Fact]
        public void ItThrowsUnexpectedStateExceptionExceptionForNewInstance()
        {
            Assert.Throws<UnexpectedStateException>(() => _strategy.ProcessMessage(null, new InstanceMessage(new ProductAddedToBasket("basketId", "productId"))));
        }

        [Fact]
        public void ItThrowsUnexpectedStateExceptionExceptionForExistingInstance()
        {
            var basket = _strategy.ProcessMessage(null, new InstanceMessage(new BasketPickedUp("basketId")));
            Assert.Throws<UnexpectedStateException>(() => _strategy.ProcessMessage(basket, new InstanceMessage(new BasketPickedUp("basketId"))));
        }

        [Fact]
        public void ItProcessesSecondMessage()
        {
            var basket = _strategy.ProcessMessage(null, new InstanceMessage(new BasketPickedUp("basketId")));
            Assert.IsAssignableFrom<CheckedOutBasket>(_strategy.ProcessMessage(basket, new InstanceMessage(new BasketCheckedOut("basketId", "orderId", new string[] {}))));
        }

        private class UnknownMessage
        {}
    }
}
