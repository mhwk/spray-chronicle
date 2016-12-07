namespace SprayChronicle.QueryHandling
{
    public class UnhandledQueryException : QueryHandlingException
    {
        public UnhandledQueryException(string message): base(message)
        {}
    }
}