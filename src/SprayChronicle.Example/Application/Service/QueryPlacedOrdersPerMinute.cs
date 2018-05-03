using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using SprayChronicle.Persistence.Raven;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Example.Application.Service
{
    public class QueryPlacedOrdersPerMinute : RavenQueries<QueryPlacedOrdersPerMinute,QueryPlacedOrdersPerMinute.Result>,
        IExecute<PerMinute>
    {
        public async Task<Executed> Execute(PerMinute query)
        {
            return await Execute<PlacedOrdersPerMinute>()
                .Query<Result>(baskets => baskets
                    .Skip(0)
                    .Take(50)
                    .ToListAsync());
        }
        
        public sealed class Result
        {
            public string Epoch { get; set; }

            public int Count { get; set; }
        }
        
        public sealed class PlacedOrdersPerMinute : AbstractIndexCreationTask<QueryPlacedOrders.PlacedOrders_v2,Result>
        {
            public PlacedOrdersPerMinute()
            {
                Map = orders =>
                    from order in orders
                    select new Result
                    {
                        Epoch = order.CheckedOutAt.ToString("yyyy-MM-dd-HH-mm"),
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
