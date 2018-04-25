using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Example.Domain.Model
{
    public abstract class Basket : EventSourced<Basket>
    {
        protected readonly BasketId BasketId;

        protected readonly ImmutableList<ProductId> ProductsInBasket;

        protected Basket(BasketId basketId, ImmutableList<ProductId> productsInBasket)
        {
            BasketId = basketId;
            ProductsInBasket = productsInBasket;
        }

        public override string Identity()
        {
            return BasketId;
        }
        
        public static Task<Basket> PickUp(BasketId basketId)
        {
            return Apply(new BasketPickedUp(
                basketId
            ));
        }

        protected static PickedUpBasket On(BasketPickedUp @event, DateTime epoch)
        {
            return new PickedUpBasket(
                new BasketId(@event.BasketId),
                ImmutableList.Create<ProductId>()
            );
        }

        protected PickedUpBasket On(ProductAddedToBasket @event, DateTime epoch)
        {
            return new PickedUpBasket(
                BasketId,
                ProductsInBasket.Add(new ProductId(@event.ProductId))
            );
        }

        protected PickedUpBasket On(ProductRemovedFromBasket @event, DateTime epoch)
        {
            return new PickedUpBasket(
                BasketId,
                ProductsInBasket.Remove(new ProductId(@event.ProductId))
            );
        }

        protected CheckedOutBasket On(BasketCheckedOut @event, DateTime epoch)
        {
            return new CheckedOutBasket(BasketId, ProductsInBasket);
        }
    }
}
