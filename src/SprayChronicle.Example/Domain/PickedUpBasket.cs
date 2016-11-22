using SprayChronicle.Example.Contracts.Events;

namespace SprayChronicle.Example.Domain
{
    public sealed class PickedUpBasket : Basket
    {
        public PickedUpBasket(BasketId basketId): base(basketId)
        {}

        public PickedUpBasket AddProduct(ProductId productId)
        {
            return (PickedUpBasket) Apply(this, new ProductAddedToBasket(
                BasketId.ToString(),
                productId.ToString()
            ));
        }

        public PickedUpBasket RemoveProduct(ProductId productId)
        {
            return (PickedUpBasket) Apply(this, new ProductRemovedFromBasket(
                BasketId.ToString(),
                productId.ToString()
            ));
        }

        public CheckedOutBasket CheckOut(OrderId orderId)
        {
            return (CheckedOutBasket) Apply(this, new BasketCheckedOut(
                BasketId.ToString(),
                orderId.ToString()
            ));
        }
    }
}
