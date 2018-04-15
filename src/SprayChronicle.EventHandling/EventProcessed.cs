using System;
using System.Threading.Tasks;

namespace SprayChronicle.EventHandling
{
    public abstract class EventProcessed
    {
        public string Identity { get; }
        
        public EventProcessed()
        {
        }
        
        public EventProcessed(string identity)
        {
            Identity = identity;
        }

        public abstract object Do(object state);
    }
    
    public sealed class EventProcessed<TState> : EventProcessed
        where TState : class
    {
        public Func<TState> Create { get; private set; }
        
        public Func<TState,TState> Update { get; private set; }
        
        public EventProcessed()
        {
        }
        
        public EventProcessed(string identity) : base(identity)
        {
        }
        
        public Task<EventProcessed<TState>> Mutate(Func<TState> mutation)
        {
            Create = mutation;

            return Task.FromResult(this);
        }
        
        public Task<EventProcessed<TState>> Mutate(Func<TState,TState> mutation)
        {
            Update = mutation;

            return Task.FromResult(this);
        }

        public override object Do(object state)
        {
            if (null != Create) {
                if (null != state) {
                    throw new Exception($"Existing state received for create");
                }
                return Create();
            }
            
            if (null != Update) {
                if (null == state) {
                    throw new Exception($"No state received for update");
                }
                return Update(state as TState);
            }

            throw new Exception($"No Create or Update task provided");
        }
    }
}
