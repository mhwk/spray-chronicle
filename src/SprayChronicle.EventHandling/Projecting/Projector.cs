namespace SprayChronicle.EventHandling.Projecting
{
    public abstract class Projector<T> : IHandleEvent
    {
        readonly IProjectionRepository<T> _repository;

        public Projector(IProjectionRepository<T> repository)
        {
            _repository = repository;
        }

        protected IProjectionRepository<T> Repository()
        {
            return _repository;
        }
    }
}
