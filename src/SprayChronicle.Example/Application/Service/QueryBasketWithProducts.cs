using System;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using SprayChronicle.EventHandling;
using SprayChronicle.Example.Application.State;
using SprayChronicle.Example.Domain;
using SprayChronicle.Persistence.Raven;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class QueryBasketWithProducts : RavenQueryProcessor<BasketWithProducts>,
        IEventProcessor<BasketPickedUp>,
        IEventProcessor<ProductAddedToBasket>,
        IEventProcessor<ProductRemovedFromBasket>,
        IQueryExecutor<BasketById>,
        IQueryExecutor<PickedUpPerDay>
    {
        public async Task Process(BasketPickedUp payload, DateTime epoch)
        {
            await For()
                .Mutate(() => new BasketWithProducts(payload.BasketId, epoch));
        }

        public async Task Process(ProductAddedToBasket payload, DateTime epoch)
        {
            await For(payload.BasketId)
                .Mutate(basket => basket.AddProductId(payload.ProductId));
        }

        public async Task Process(ProductRemovedFromBasket payload, DateTime epoch)
        {
            await For(payload.BasketId)
                .Mutate(basket => basket.RemoveProductId(payload.ProductId));
        }
        
        public async Task<QueryMetadata> Execute(BasketById query)
        {
            return await For()
                .Query<QueryBasketWithProducts_BasketById>(baskets => baskets
                    .Where(b => b.BasketId == query.BasketId, false)
                    .FirstOrDefaultAsync());
        }

        public async Task<QueryMetadata> Execute(PickedUpPerDay query)
        {
            return await For<PickedUpBasketsPerDay>()
                .Query<QueryBasketWithProducts_PickedUpPerDay>(baskets => baskets
                    .Skip((query.Page - 1) * 50)
                    .Take(50)
                    .ToListAsync());
        }

        public void SubscribeTo(SubscriptionRouter router)
        {
            router.Subscribe(this);
        }
    }
}
