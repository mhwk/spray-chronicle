namespace SprayChronicle.MessageHandling
{
    public sealed class UnhandledMessageException : MessageHandlingException
    {
        public UnhandledMessageException(string message): base(message)
        {}
    }
}