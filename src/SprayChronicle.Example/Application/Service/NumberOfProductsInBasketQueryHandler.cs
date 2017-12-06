using System;
using System.Linq;
using SprayChronicle.QueryHandling;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.State;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class NumberOfProductsInBasketQueryHandler : QueryHandler<NumberOfProductsInBasket>
    {
        public NumberOfProductsInBasketQueryHandler(IStatefulRepository<NumberOfProductsInBasket> repository): base(repository)
        {}
        
        private void Process(BasketPickedUp @event, DateTime at)
        {
            Repository().Start(() => new NumberOfProductsInBasket(@event.BasketId));
        }

        private void Process(ProductAddedToBasket @event, DateTime at)
        {
            Repository().With(@event.BasketId, basket => basket.Increase());
        }

        private void Process(ProductRemovedFromBasket @event, DateTime at)
        {
            Repository().With(@event.BasketId, basket => basket.Decrease());
        }

        private object Execute(NumberOfProductsForBasketId query)
        {
            return Repository().Load(q => q.FirstOrDefault(item => item.BasketId == query.BasketId));
        }

        private object Execute(NumberOfProductsInBaskets query)
        {
            return Repository().Load(q => q);
        }

        private object Execute(PagedNumberOfProductsInBasket query)
        {
            return Repository().Load(q => q, query.Page, query.PerPage);
        }
    }
}
