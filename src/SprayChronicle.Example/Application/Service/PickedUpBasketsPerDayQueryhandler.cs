using System;
using System.Linq;
using SprayChronicle.QueryHandling;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.State;

namespace SprayChronicle.Example.Application.Service
{
    public class PickedUpBasketsPerDayQueryHandler : QueryHandler<PickedUpBasketsPerDay>
    {
        public PickedUpBasketsPerDayQueryHandler(IStatefulRepository<PickedUpBasketsPerDay> repository)
            : base(repository)
        {}
        
        private PickedUpBasketsPerDay FindOrCreate(DateTime epoch)
        {
            return Repository().Load(q => q.FirstOrDefault(i => i.Day == epoch.ToString("yyyy-MM-dd")))
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
