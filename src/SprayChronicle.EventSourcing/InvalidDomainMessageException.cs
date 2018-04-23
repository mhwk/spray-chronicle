namespace SprayChronicle.EventSourcing
{
    public sealed class InvalidDomainMessageException : EventSourcingException
    {
        public InvalidDomainMessageException(string message) : base(message)
        {
        }
    }
}
