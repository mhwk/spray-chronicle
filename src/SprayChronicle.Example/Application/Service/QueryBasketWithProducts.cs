using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SprayChronicle.EventHandling;
using SprayChronicle.Example.Domain;
using SprayChronicle.Persistence.Raven;
using SprayChronicle.QueryHandling;
using Processed = SprayChronicle.EventHandling.Processed;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class QueryBasketWithProducts : RavenQueries<QueryBasketWithProducts,QueryBasketWithProducts.BasketWithProducts_v1>,
        IProcess<BasketPickedUp>,
        IProcess<ProductAddedToBasket>,
        IProcess<ProductRemovedFromBasket>,
        IExecute<BasketById>
    {
        public async Task<Processed> Process(BasketPickedUp payload, DateTime epoch)
        {
            return await Process(payload.BasketId)
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
        
        public async Task<Executed> Execute(BasketById query)
        {
            return await Execute()
                .Find(query.BasketId);
        }
        
        public sealed class BasketWithProducts_v1
        {
            public string BasketId { get; }
        
            public DateTime PickedUpAt { get; }

            public readonly List<string> ProductIds = new List<string>();

            public BasketWithProducts_v1(string basketId, DateTime pickedUpAt)
            {
                BasketId = basketId;
                PickedUpAt = pickedUpAt;
            }

            public BasketWithProducts_v1 AddProductId(string productId)
            {
                ProductIds.Add(productId);
                return this;
            }

            public BasketWithProducts_v1 RemoveProductId(string productId)
            {
                ProductIds.Remove(productId);
                return this;
            }
        }
    }
}
