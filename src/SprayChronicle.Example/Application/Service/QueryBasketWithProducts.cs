using System;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using SprayChronicle.EventHandling;
using SprayChronicle.Example.Application.State;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.Persistence.Raven;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class QueryBasketWithProducts : RavenQueryProcessor<QueryBasketWithProducts,BasketWithProducts>,
        IProcess<BasketPickedUp>,
        IProcess<ProductAddedToBasket>,
        IProcess<ProductRemovedFromBasket>,
        IExecute<BasketById>,
        IExecute<PickedUpPerDay>
    {
        public async Task<EventProcessed> Process(BasketPickedUp payload, DateTime epoch)
        {
            return await Process()
                .Mutate(() => new BasketWithProducts(payload.BasketId, epoch));
        }

        public async Task<EventProcessed> Process(ProductAddedToBasket payload, DateTime epoch)
        {
            return await Process(payload.BasketId)
                .Mutate(basket => basket.AddProductId(payload.ProductId));
        }

        public async Task<EventProcessed> Process(ProductRemovedFromBasket payload, DateTime epoch)
        {
            return await Process(payload.BasketId)
                .Mutate(basket => basket.RemoveProductId(payload.ProductId));
        }
        
        public async Task<QueryExecuted> Execute(BasketById query)
        {
            return await Execute<QueryBasketWithProducts_BasketById>()
                .Query(baskets => baskets
                    .Where(b => b.BasketId == query.BasketId, false)
                    .FirstOrDefaultAsync());
        }

        public async Task<QueryExecuted> Execute(PickedUpPerDay query)
        {
            return await Execute<QueryBasketWithProducts_PickedUpPerDay>()
                .Query<PickedUpBasketsPerDay>(baskets => baskets
                    .Skip((query.Page - 1) * 50)
                    .Take(50)
                    .ToListAsync());
        }
    }
}
