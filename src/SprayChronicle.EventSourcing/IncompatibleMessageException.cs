namespace SprayChronicle.EventSourcing
{
    public sealed class IncompatibleMessageException : EventSourcingException
    {
        public IncompatibleMessageException(string message): base(message)
        {
        }
    }
}