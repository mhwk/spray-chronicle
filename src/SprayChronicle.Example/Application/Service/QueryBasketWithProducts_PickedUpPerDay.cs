using System;
using System.Linq;
using Raven.Client.Documents.Indexes;
using SprayChronicle.Example.Application.State;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class QueryBasketWithProducts_PickedUpPerDay : AbstractIndexCreationTask<BasketWithProducts_v1,PickedUpBasketsPerDay_v1>
    {
        public QueryBasketWithProducts_PickedUpPerDay()
        {
            Map = baskets =>
                from basket in baskets
                select new PickedUpBasketsPerDay_v1
                {
                    Day = basket.PickedUpAt.ToString("yyyy-MM-dd"),
                    Count = 1
                };
                
            Reduce = results =>
                from result in results
                group result by result.Day into g
                select new PickedUpBasketsPerDay_v1
                {
                    Day = g.Key,
                    Count = g.Sum(r => r.Count)
                };
        }
    }
}
