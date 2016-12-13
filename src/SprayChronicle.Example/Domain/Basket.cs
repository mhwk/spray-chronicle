using System.Collections.Immutable;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Contracts.Events;

namespace SprayChronicle.Example.Domain
{
    public abstract class Basket : EventSourced<Basket>
    {
        protected readonly BasketId BasketId;

        protected readonly ImmutableList<ProductId> ProductsInBasket; 

        public Basket(BasketId basketId, ImmutableList<ProductId> productsInBasket)
        {
            BasketId = basketId;
            ProductsInBasket = productsInBasket;
        }

        public override string Identity()
        {
            return BasketId;
        }

        public static PickedUpBasket PickUp(BasketId basketId)
        {
            return (PickedUpBasket) Apply(null, new BasketPickedUp(
                basketId
            ));
        }

        static PickedUpBasket On(BasketPickedUp @event)
        {
            return new PickedUpBasket(
                new BasketId(@event.BasketId),
                ImmutableList.Create<ProductId>()
            );
        }

        protected PickedUpBasket On(ProductAddedToBasket @event)
        {
            return new PickedUpBasket(
                BasketId,
                ProductsInBasket.Add(new ProductId(@event.ProductId))
            );
        }

        protected PickedUpBasket On(ProductRemovedFromBasket @event)
        {
            return new PickedUpBasket(
                BasketId,
                ProductsInBasket.Remove(new ProductId(@event.ProductId))
            );
        }

        protected CheckedOutBasket On(BasketCheckedOut @event)
        {
            return new CheckedOutBasket(BasketId, ProductsInBasket);
        }
    }
}
