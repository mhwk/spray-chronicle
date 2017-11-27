namespace SprayChronicle.MessageHandling
{
    public sealed class UnexpectedStateException : MessageHandlingException
    {
        public UnexpectedStateException(string message): base(message)
        {}
    }
}