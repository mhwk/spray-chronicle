using System.Collections.Immutable;

namespace SprayChronicle.Example.Domain.Model
{
    public sealed class CheckedOutBasket : Basket
    {
        public CheckedOutBasket(BasketId basketId, ImmutableList<ProductId> productsInBasket)
            : base(basketId, productsInBasket)
        {}
        
        public CheckedOutBasket CheckOut(OrderId orderId)
        {
            return this;
        }
    }
}
