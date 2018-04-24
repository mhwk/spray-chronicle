using System.Linq;
using Raven.Client.Documents.Indexes;
using SprayChronicle.Example.Application.State;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class QueryBasketWithProducts_BasketById : AbstractIndexCreationTask<BasketWithProducts_v1>
    {
        public QueryBasketWithProducts_BasketById()
        {
            Map = baskets =>
                from basket in baskets
                select new
                {
                    BasketId = basket.Id
                };
        }
    }
}
