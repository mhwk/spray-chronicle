using System.Linq;
using Raven.Client.Documents.Indexes;
using SprayChronicle.Example.Application.State;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class QueryBasketWithProducts_BasketById : AbstractIndexCreationTask<BasketWithProducts>
    {
        public QueryBasketWithProducts_BasketById()
        {
            Map = baskets =>
                from basket in baskets
                select new
                {
                    basket.BasketId
                };
        }
    }
}
