using System;
using SprayChronicle.EventHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Projecting
{
    public abstract class Projector<T> : IHandleEvent
    {
        readonly IStatefulRepository<T> _repository;

        public Projector(IStatefulRepository<T> repository)
        {
            _repository = repository;
        }

        protected IStatefulRepository<T> Repository()
        {
            return _repository;
        }

        protected void With(string id, Action callback)
        {
            var projection = _repository.Load(id);
            if (null == projection) {
                
            }
        }
    }
}
