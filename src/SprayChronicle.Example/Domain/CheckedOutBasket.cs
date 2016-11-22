namespace SprayChronicle.Example.Domain
{
    public sealed class CheckedOutBasket : Basket
    {
        public CheckedOutBasket(BasketId basketId): base(basketId)
        {}
    }
}
