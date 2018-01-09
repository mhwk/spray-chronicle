using System;
using System.Linq;
using SprayChronicle.QueryHandling;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.State;

namespace SprayChronicle.Example.Application.Service
{
    public class QueryPickedUpBasketsPerDay : QueryHandler<PickedUpBasketsPerDay>
    {
        public QueryPickedUpBasketsPerDay(IStatefulRepository<PickedUpBasketsPerDay> repository)
            : base(repository)
        {}
        
        private PickedUpBasketsPerDay FindOrCreate(DateTime epoch)
        {
            Console.WriteLine(epoch.ToString("yyyy-MM-dd"));
            return Execute(new PickedUpBasketsOnDay(epoch.ToString("yyyy-MM-dd")))
                ?? new PickedUpBasketsPerDay(epoch.ToString("yyyy-MM-dd"));
        }

        private void Process(BasketPickedUp @event, DateTime at)
        {
            Repository().Save(FindOrCreate(at).Increase());
        }

        private PickedUpBasketsPerDay Execute(PickedUpBasketsOnDay query)
        {
            return Repository().Load(q => q.FirstOrDefault(item => item.Day == query.Day));
        }
    }
}
