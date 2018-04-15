namespace SprayChronicle.EventHandling
{
    public interface IEventSourceFactory<out TMessage>
    {
        IEventSource<TMessage> Build<TOptions>(TOptions options);
    }
}
