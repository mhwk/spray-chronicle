namespace SprayChronicle.EventHandling.Projecting
{
    public abstract class Projector<T> : StreamEventHandler
    {
        readonly IProjectionRepository<T> _repository;

        public Projector(IStream stream, IProjectionRepository<T> repository): base(stream)
        {
            _repository = repository;
        }

        protected IProjectionRepository<T> Repository()
        {
            return _repository;
        }
    }
}
