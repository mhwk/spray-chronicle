using System;

namespace SprayChronicle
{
    public interface IEventSourcingBuilder
    {
        IEventSourcingBuilder MapState(Action<IMapState> map);
        
        IEventSourcingBuilder AddInvariant<TInvariant>()
            where TInvariant : class, IArrange<TInvariant>, IAct<TInvariant>;

        IEventSourcingBuilder AddProjector<TProjector>(int batchSize, TimeSpan timeout)
            where TProjector : class, IProject;

        IEventSourcingBuilder AddProjector<TProjector>(TimeSpan timeout)
            where TProjector : class, IProject;

        IEventSourcingBuilder AddProjector<TProjector>(int batchSize)
            where TProjector : class, IProject;

        IEventSourcingBuilder AddProcessor<TProcessor>(bool failOnError)
            where TProcessor : class, IProcess;
    }
}
