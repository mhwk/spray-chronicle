namespace SprayChronicle.EventSourcing
{
    public class InvalidStateException : EventSourcingException
    {
        public InvalidStateException(string message): base(message)
        {}
    }
}