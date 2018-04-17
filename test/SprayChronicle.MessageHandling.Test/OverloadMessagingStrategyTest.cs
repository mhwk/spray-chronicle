using System.Threading.Tasks;
using Shouldly;
using Xunit;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.MessageHandling.Test
{
    public class OverloadMessagingStrategyTest
    {
        private readonly OverloadMessagingStrategy<Basket> _strategy = new OverloadMessagingStrategy<Basket>(new ContextTypeLocator<Basket>());

        [Fact]
        public void ItAcceptsFactoryMessageForNull()
        {
            _strategy
                .Resolves(null, new BasketPickedUp("basketId"))
                .ShouldBeTrue();
        }

        [Fact]
        public void ItDoesNotAcceptInstanceMessageForNull()
        {
            _strategy
                .Resolves(null, new ProductAddedToBasket("basketId", "productId"))
                .ShouldBeFalse();
        }

        [Fact]
        public void ItDoesNotAcceptUnknownMessage()
        {
            _strategy
                .Resolves(null, new UnknownMessage())
                .ShouldBeFalse();
        }

        [Fact]
        public void ItReturnsAcceptedMethodResult()
        {
            _strategy
                .Resolves(null, new BasketPickedUp("basketId"))
                .ShouldBeAssignableTo<PickedUpBasket>();
        }

        [Fact]
        public void ItThrowsUnhandledMessageException()
        {
            Should.Throw<UnhandledMessageException>(() => _strategy.Resolves(null, new {}));
        }

        [Fact]
        public void ItThrowsUnexpectedStateExceptionExceptionForNewInstance()
        {
            Should.Throw<UnexpectedStateException>(() => _strategy.Resolves(null, new ProductAddedToBasket("basketId", "productId")));
        }

        [Fact]
        public async Task ItThrowsUnexpectedStateExceptionExceptionForExistingInstance()
        {
            var basket = await _strategy.Ask<Basket>(null, new BasketPickedUp("basketId"));
               
            Should.Throw<UnexpectedStateException>(_strategy.Ask<Basket>(basket, new BasketPickedUp("basketId")));
        }

        [Fact]
        public async Task ItProcessesSecondMessage()
        {
            var basket = await _strategy.Ask<Basket>(null, new BasketPickedUp("basketId"));
            
            _strategy
                .Ask<Basket>(basket, new BasketCheckedOut("basketId", "orderId", new string[] {}))
                .ShouldBeAssignableTo<CheckedOutBasket>();
        }

        private class UnknownMessage
        {}
    }
}
