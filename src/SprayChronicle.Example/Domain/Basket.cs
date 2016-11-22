using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Contracts.Events;

namespace SprayChronicle.Example.Domain
{
    public abstract class Basket : EventSourced<Basket>
    {
        protected readonly BasketId BasketId;

        public Basket(BasketId basketId)
        {
            BasketId = basketId;
        }

        public override string Identity()
        {
            return BasketId.ToString();
        }

        public static PickedUpBasket PickUp(BasketId basketId)
        {
            return (PickedUpBasket) Apply(null, new BasketPickedUp(
                basketId.ToString()
            ));
        }

        static PickedUpBasket On(BasketPickedUp @event)
        {
            return new PickedUpBasket(new BasketId(@event.BasketId));
        }

        protected PickedUpBasket On(ProductAddedToBasket @event)
        {
            return new PickedUpBasket(BasketId);
        }

        protected PickedUpBasket On(ProductRemovedFromBasket @event)
        {
            return new PickedUpBasket(BasketId);
        }

        protected CheckedOutBasket On(BasketCheckedOut @event)
        {
            return new CheckedOutBasket(BasketId);
        }
    }
}
