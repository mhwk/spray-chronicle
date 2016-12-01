using Microsoft.Extensions.Logging;

namespace SprayChronicle.EventHandling.Projecting
{
    public abstract class Projector<T> : StreamEventHandler
    {
        readonly IProjectionRepository<T> _repository;

        public Projector(ILogger<StreamEventHandler> logger, IStream stream, IProjectionRepository<T> repository)
            : base(logger, stream)
        {
            _repository = repository;
        }

        protected IProjectionRepository<T> Repository()
        {
            return _repository;
        }
    }
}
