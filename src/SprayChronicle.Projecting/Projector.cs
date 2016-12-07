using SprayChronicle.EventHandling;

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
    }
}
