using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Sparrow.Platform.Posix.macOS;
using SprayChronicle.QueryHandling;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.State;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class QueryBasketWithProducts : IHandleQueries
    {
        private readonly IDocumentStore _store;

        public QueryBasketWithProducts(IDocumentStore store)
        {
            _store = store;
        }
        
        private void Process(BasketPickedUp @event, DateTime at)
        {
            Repository().Start(() => new BasketWithProducts(@event.BasketId, at));
        }

        private void Process(ProductAddedToBasket @event, DateTime at)
        {
            Repository().With(@event.BasketId, basket => basket.AddProductId(@event.ProductId));
        }

        private void Process(ProductRemovedFromBasket @event, DateTime at)
        {
            Repository().With(@event.BasketId, basket => basket.RemoveProductId(@event.ProductId));
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
        
        public sealed class BasketWithProducts
        {
            public string BasketId { get; }
            
            public DateTime PickedUpAt { get; }

            public readonly List<string> ProductIds = new List<string>();

            public BasketWithProducts(string basketId, DateTime pickedUpAt)
            {
                BasketId = basketId;
                PickedUpAt = pickedUpAt;
            }

            public BasketWithProducts AddProductId(string productId)
            {
                ProductIds.Add(productId);
                return this;
            }

            public BasketWithProducts RemoveProductId(string productId)
            {
                ProductIds.Remove(productId);
                return this;
            }
        }

        public sealed class BasketWithProducts_ByBasketId : AbstractIndexCreationTask<BasketWithProducts>
        {
            public BasketWithProducts_ByBasketId()
            {
                Map = baskets =>
                    from basket in baskets
                    select new
                    {
                        basket.BasketId
                    };
            }
        }

        public sealed class BasketWithProducts_NumberOfProducts : AbstractIndexCreationTask<BasketWithProducts, BasketWithProducts_NumberOfProducts.Result>
        {
            public sealed class Result
            {
                public double ProductCount { get; set; }
            }

            public BasketWithProducts_NumberOfProducts()
            {
                Map = baskets =>
                    from basket in baskets
                    select new Result
                    {
                        ProductCount = basket.ProductIds.Count
                    };
            }
        }

        public sealed class BasketWithProducts_PickedUpPerDay : AbstractIndexCreationTask<BasketWithProducts, BasketWithProducts_PickedUpPerDay.Result>
        {
            public sealed class Result
            {
                public DateTime PickedUpAt { get; set; }
                public int Count { get; set; }
            }

            public BasketWithProducts_PickedUpPerDay()
            {
                Map = baskets =>
                    from basket in baskets
                    select new Result
                    {
                        PickedUpAt = basket.PickedUpAt,
                        Count = 1
                    };
                
                Reduce = results =>
                    from result in results
                    group result by result.PickedUpAt into g
                    select new Result
                    {
                        PickedUpAt = g.Key,
                        Count = g.Sum(r => r.Count)
                    };
            }
        }

        public bool Executes(object query)
        {
            return true;
        }

        public object Execute(object query)
        {
            
        }

        public bool Processes(object @event, DateTime at)
        {
            return true;
        }

        public void Process(object @event, DateTime at)
        {
            switch (@event)
            {
                case BasketPickedUp message:

                    break;
                case ProductAddedToBasket message:

                    break;
                case ProductRemovedFromBasket message:

                    break;
            }
        }
    }
}
