namespace SprayChronicle.QueryHandling
{
    public sealed class UnhandledQueryException : QueryHandlingException
    {
        public UnhandledQueryException(string message): base(message)
        {}
    }
}