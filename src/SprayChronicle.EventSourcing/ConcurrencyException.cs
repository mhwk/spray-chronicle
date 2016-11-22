namespace SprayChronicle.EventSourcing
{
    public class ConcurrencyException : EventSourcingException
    {
        public ConcurrencyException(string message): base(message)
        {}
    }
}
