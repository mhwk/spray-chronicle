namespace SprayChronicle.EventSourcing
{
    public interface IEventSourceFactory
    {
        IEventSource<TTarget> Build<TTarget,TOptions>(TOptions options) where TTarget : class;
    }
}
