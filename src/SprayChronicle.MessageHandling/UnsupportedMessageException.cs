namespace SprayChronicle.MessageHandling
{
    public sealed class UnsupportedMessageException : MessageHandlingException
    {
        public UnsupportedMessageException(string message) : base(message)
        {
        }
    }
}