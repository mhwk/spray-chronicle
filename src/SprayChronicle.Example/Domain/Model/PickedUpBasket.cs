using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace SprayChronicle.Example.Domain.Model
{
    public sealed class PickedUpBasket : Basket
    {
        public PickedUpBasket(BasketId basketId, ImmutableList<ProductId> productsInBasket)
            : base(basketId, productsInBasket)
        {}
        
        public PickedUpBasket(BasketId basketId)
            : base(basketId, ImmutableList<ProductId>.Empty)
        {}

        public async Task<Basket> AddProduct(ProductId productId)
        {
            return await Apply(this, new ProductAddedToBasket(
                BasketId.ToString(),
                productId.ToString()
            ));
        }

        public  async Task<Basket> RemoveProduct(ProductId productId)
        {
            AssertContainsProduct(productId);
            
            return await Apply(this, new ProductRemovedFromBasket(
                BasketId.ToString(),
                productId.ToString()
            ));
        }

        public async Task<Basket> CheckOut(OrderId orderId)
        {
            return await Apply(this, new BasketCheckedOut(
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
