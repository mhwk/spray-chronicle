using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using SprayChronicle.Persistence.Raven;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class QueryBasketsPickedUpPerDay : RavenQueries<QueryBasketsPickedUpPerDay,QueryBasketsPickedUpPerDay.Result>,
        IExecute<PerDay>
    {
        public async Task<Executed> Execute(PerDay query)
        {
            return await Execute<BasketsPickedUpPerDay>()
                .Query<Result>(orders => orders
                    .Skip(0)
                    .Take(50)
                    .ToListAsync());
        }
        
        public sealed class Result
        {
            public string Epoch { get; set; }

            public int Count { get; set; }
        }
        
        public sealed class BasketsPickedUpPerDay : AbstractIndexCreationTask<QueryBasketWithProducts.BasketWithProducts_v1,Result>
        {

            public BasketsPickedUpPerDay()
            {
                Map = baskets =>
                    from basket in baskets
                    select new Result
                    {
                        Epoch = basket.PickedUpAt.ToString("yyyy-MM-dd"),
                        Count = 1
                    };
                
                Reduce = results =>
                    from result in results
                    group result by result.Epoch into g
                    select new Result
                    {
                        Epoch = g.Key,
                        Count = g.Sum(r => r.Count)
                    };
            }
        }
    }
}
