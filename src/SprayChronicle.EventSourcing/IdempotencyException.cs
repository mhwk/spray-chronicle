namespace SprayChronicle.EventSourcing
{
    public sealed class IdempotencyException : EventSourcingException
    {
        public IdempotencyException(string message) : base(message)
        {
        }
    }
}
