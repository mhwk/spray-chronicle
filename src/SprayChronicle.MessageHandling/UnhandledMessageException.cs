namespace SprayChronicle.MessageHandling
{
    public class UnhandledMessageException : MessageHandlingException
    {
        public UnhandledMessageException(string message): base(message)
        {}
    }
}