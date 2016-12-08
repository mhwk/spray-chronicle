namespace SprayChronicle.MessageHandling
{
    public class UnexpectedStateException : MessageHandlingException
    {
        public UnexpectedStateException(string message): base(message)
        {}
    }
}