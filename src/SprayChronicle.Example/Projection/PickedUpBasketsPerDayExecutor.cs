using System.Linq;
using SprayChronicle.QueryHandling;
using SprayChronicle.Example.Contracts.Queries;

namespace SprayChronicle.Example.Projection
{
    public class PickedUpBasketsPerDayExecutor : OverloadQueryExecutor<PickedUpBasketsPerDay>
    {
        public PickedUpBasketsPerDayExecutor(IStatefulRepository<PickedUpBasketsPerDay> repository)
            : base(repository)
        {}

        public PickedUpBasketsPerDay On(PickedUpBasketsOnDay query)
        {
            return _repository.Load(q => q.FirstOrDefault(item => item.Day == query.Day));
        }
    }
}
