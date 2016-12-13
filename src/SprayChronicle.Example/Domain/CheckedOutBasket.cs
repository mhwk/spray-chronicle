using System.Collections.Immutable;

namespace SprayChronicle.Example.Domain
{
    public sealed class CheckedOutBasket : Basket
    {
        public CheckedOutBasket(BasketId basketId, ImmutableList<ProductId> productsInBasket)
            : base(basketId, productsInBasket)
        {}
    }
}
