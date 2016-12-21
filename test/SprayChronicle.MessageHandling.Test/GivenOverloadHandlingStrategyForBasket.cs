using Xunit;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Contracts.Events;

namespace SprayChronicle.MessageHandling.Test
{
    public class GivenOverloadHandlingStrategyForBasket
    {
        public OverloadHandlingStrategy<Basket> Strategy;

        public GivenOverloadHandlingStrategyForBasket()
        {
            Strategy = new OverloadHandlingStrategy<Basket>();
        }

        [Fact]
        public void ItAcceptsInitialMessage()
        {
            Assert.True(Strategy.AcceptsMessage(null, new BasketPickedUp("basketId")));
        }

        [Fact]
        public void ItDoesNotAcceptSecondMessage()
        {
            Assert.False(Strategy.AcceptsMessage(null, new ProductAddedToBasket("basketId", "productId")));
        }

        [Fact]
        public void ItDoesNotAcceptUnknownMessage()
        {
            Assert.False(Strategy.AcceptsMessage(null, new UnknownMessage()));
        }

        [Fact]
        public void ItReturnsAcceptedMethodResult()
        {
            Assert.IsAssignableFrom<PickedUpBasket>(Strategy.ProcessMessage(null, new BasketPickedUp("basketId")));
        }

        [Fact]
        public void ItThrowsUnhandledMessageException()
        {
            Assert.Throws<UnhandledMessageException>(() => Strategy.ProcessMessage(null, new {}));
        }

        [Fact]
        public void ItThrowsUnexpectedStateExceptionExceptionForNewInstance()
        {
            Assert.Throws<UnexpectedStateException>(() => Strategy.ProcessMessage(null, new ProductAddedToBasket("basketId", "productId")));
        }

        [Fact]
        public void ItThrowsUnexpectedStateExceptionExceptionForExistingInstance()
        {
            var basket = Strategy.ProcessMessage(null, new BasketPickedUp("basketId"));
            Assert.Throws<UnexpectedStateException>(() => Strategy.ProcessMessage(basket, new BasketPickedUp("basketId")));
        }

        [Fact]
        public void ItProcessesSecondMessage()
        {
            var basket = Strategy.ProcessMessage(null, new BasketPickedUp("basketId"));
            Assert.IsAssignableFrom<CheckedOutBasket>(Strategy.ProcessMessage(basket, new BasketCheckedOut("basketId", "orderId")));
        }

        public sealed class UnknownMessage
        {

        }
    }
}
