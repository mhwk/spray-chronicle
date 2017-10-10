using System;
using SprayChronicle.EventHandling;
using SprayChronicle.Projecting;
using SprayChronicle.QueryHandling;
using SprayChronicle.Example.Application.Model;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.Example.Application.Effect
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
            Start(() => new NumberOfProductsInBasket(@event.BasketId));
        }

        public void On(ProductAddedToBasket @event, DateTime epoch)
        {
            With(@event.BasketId, basket => basket.Increase());
        }

        public void On(ProductRemovedFromBasket @event, DateTime epoch)
        {
            With(@event.BasketId, basket => basket.Decrease());
        }
    }
}