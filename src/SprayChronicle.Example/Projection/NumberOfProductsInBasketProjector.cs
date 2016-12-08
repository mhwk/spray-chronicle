using System;
using SprayChronicle.EventHandling;
using SprayChronicle.Projecting;
using SprayChronicle.QueryHandling;
using SprayChronicle.Example.Contracts.Events;

namespace SprayChronicle.Example.Projection
{
    public class NumberOfProductsInBasketProjector
        : Projector<NumberOfProductsInBasket>,
          IHandleEvent<BasketPickedUp>,
          IHandleEvent<ProductAddedToBasket>,
          IHandleEvent<ProductRemovedFromBasket>
    {
        public NumberOfProductsInBasketProjector(IStatefulRepository<NumberOfProductsInBasket> repository)
            : base(repository)
        {}

        public void On(BasketPickedUp @event, DateTime epoch)
        {
            Repository().Save(new NumberOfProductsInBasket(@event.BasketId));
        }

        public void On(ProductAddedToBasket @event, DateTime epoch)
        {
            Repository().Save(
                Repository().Load(@event.BasketId).Increase()
            );
        }

        public void On(ProductRemovedFromBasket @event, DateTime epoch)
        {
            Repository().Save(
                Repository().Load(@event.BasketId).Decrease()
            );
        }
    }
}