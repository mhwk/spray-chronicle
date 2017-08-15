using System;
using System.Linq;
using SprayChronicle.Projecting;
using SprayChronicle.QueryHandling;
using SprayChronicle.Example.Contracts.Events;

namespace SprayChronicle.Example.Projection
{
    public class PickedUpBasketsPerDayProjector : Projector<PickedUpBasketsPerDay>
    {
        public PickedUpBasketsPerDayProjector(IStatefulRepository<PickedUpBasketsPerDay> repository)
            : base(repository)
        {}

        private PickedUpBasketsPerDay FindOrCreate(DateTime epoch)
        {
            var item = Repository().Load(q => q.FirstOrDefault(i => i.Day == epoch.ToString("yyyy-MM-dd")));
            return item ?? new PickedUpBasketsPerDay(epoch.ToString("yyyy-MM-dd"));
        }

        public void On(BasketPickedUp @event, DateTime epoch)
        {
            Repository().Save(FindOrCreate(epoch).Increase());
        }
    }
}
