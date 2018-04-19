namespace SprayChronicle.MessageHandling
{
    public sealed class UnroutableMessageException : MessageHandlingException
    {
        public UnroutableMessageException(string message): base(message)
        {}
    }
}