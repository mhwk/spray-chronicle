namespace SprayChronicle.MessageHandling
{
    public sealed class IncompatibleMessageException : MessageHandlingException
    {
        public IncompatibleMessageException(string message): base(message)
        {
        }
    }
}