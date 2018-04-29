using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using SprayChronicle.Persistence.Raven;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class QueryPlacedOrdersPerDay : RavenQueries<QueryPlacedOrdersPerDay,QueryPlacedOrdersPerDay.Result>,
        IExecute<PerDay>
    {
        public async Task<Executor> Execute(PerDay query)
        {
            return await Execute<PlacedOrdersPerDay_v1>()
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
        
        public class PlacedOrdersPerDay_v1 : AbstractIndexCreationTask<QueryPlacedOrders.PlacedOrders_v3,Result>
        {
            public PlacedOrdersPerDay_v1()
            {
                Map = orders =>
                    from order in orders
                    select new Result
                    {
                        Epoch = order.CheckedOutAt.ToString("yyyy-MM-dd"),
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
