using System;
using SprayChronicle.EventHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Projecting
{
    public abstract class Projector<T> : IHandleEvent
    {
        protected readonly IStatefulRepository<T> _repository;

        public Projector(IStatefulRepository<T> repository)
        {
            _repository = repository;
        }

        protected IStatefulRepository<T> Repository()
        {
            return _repository;
        }

        protected void Start(Func<T> callback)
        {
            _repository.Save(callback());
        }

        protected void With(string id, Func<T,T> callback)
        {
            var projection = _repository.Load(id);
            if (null == projection) {
                throw new ProjectionException(string.Format(
                    "Projection {0} with id {1} does not exist", typeof(T), id
                ));
            }
            _repository.Save(callback(projection));
        }

        protected void End(string id)
        {
            _repository.Remove(id);
        }

    }
}
