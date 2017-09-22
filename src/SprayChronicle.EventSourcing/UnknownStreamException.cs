namespace SprayChronicle.EventSourcing
{
    public class UnknownStreamException : EventSourcingException
    {
        public UnknownStreamException(string message): base(message)
        {}
    }
}
