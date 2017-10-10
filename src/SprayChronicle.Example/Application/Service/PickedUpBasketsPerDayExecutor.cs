using System.Linq;
using SprayChronicle.QueryHandling;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Model;

namespace SprayChronicle.Example.Application.Service
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
