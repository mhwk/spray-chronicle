namespace SprayChronicle.Server
{
    public sealed class ConsoleLoggerFactory : ILoggerFactory
    {
        public ILogger<T> Create<T>()
        {
            return new ConsoleLogger<T>();
        }
    }
}