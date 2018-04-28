namespace SprayChronicle.CommandHandling
{
    public sealed class InvalidCommandException : CommandHandlingException
    {
        public InvalidCommandException(string message, string messageId, string causationId, string correlationId) : base(message, messageId, causationId, correlationId)
        {
        }
    }
}
