using System.Collections.Immutable;
using System.Linq;

namespace SprayChronicle.Example.Domain.Model
{
    public sealed class PickedUpBasket : Basket
    {
        public PickedUpBasket(BasketId basketId, ImmutableList<ProductId> productsInBasket)
            : base(basketId, productsInBasket)
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
            AssertContainsProduct(productId);
            
            return (PickedUpBasket) Apply(this, new ProductRemovedFromBasket(
                BasketId.ToString(),
                productId.ToString()
            ));
        }

        public CheckedOutBasket CheckOut(OrderId orderId)
        {
            return (CheckedOutBasket) Apply(this, new BasketCheckedOut(
                BasketId.ToString(),
                orderId.ToString(),
                ProductsInBasket.Select(p => p.ToString()).ToArray()
            ));
        }

        private void AssertContainsProduct(ProductId productId)
        {
            if ( ! ProductsInBasket.Contains(productId)) {
                throw new ProductNotInBasketException(string.Format(
                    "Product with id {0} is not in the basket",
                    productId
                ));
            }
        }
    }
}
