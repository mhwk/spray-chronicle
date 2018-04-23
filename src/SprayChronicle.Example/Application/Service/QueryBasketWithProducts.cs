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
    public sealed class QueryBasketWithProducts : RavenQueryProcessor<QueryBasketWithProducts,BasketWithProducts_v1>,
        IProcess<BasketPickedUp>,
        IProcess<ProductAddedToBasket>,
        IProcess<ProductRemovedFromBasket>,
        IExecute<BasketById>,
        IExecute<PickedUpPerDay>
    {
        public async Task<Processed> Process(BasketPickedUp payload, DateTime epoch)
        {
            return await Process()
                .Mutate(() => new BasketWithProducts_v1(payload.BasketId, epoch));
        }

        public async Task<Processed> Process(ProductAddedToBasket payload, DateTime epoch)
        {
            return await Process(payload.BasketId)
                .Mutate(basket => basket.AddProductId(payload.ProductId));
        }

        public async Task<Processed> Process(ProductRemovedFromBasket payload, DateTime epoch)
        {
            return await Process(payload.BasketId)
                .Mutate(basket => basket.RemoveProductId(payload.ProductId));
        }
        
        public async Task<Executor> Execute(BasketById query)
        {
            return await Execute<QueryBasketWithProducts_BasketById>()
                .Query(baskets => baskets
                    .Where(b => b.BasketId == query.BasketId, false)
                    .FirstOrDefaultAsync());
        }

        public async Task<Executor> Execute(PickedUpPerDay query)
        {
            return await Execute<QueryBasketWithProducts_PickedUpPerDay>()
                .Query<PickedUpBasketsPerDay_v1>(baskets => baskets
                    .Skip((query.Page - 1) * 50)
                    .Take(50)
                    .ToListAsync());
        }
    }
}
