namespace SprayChronicle.Server
{
    public interface ILoggerFactory
    {
        ILogger<T> Create<T>();
    }
}
