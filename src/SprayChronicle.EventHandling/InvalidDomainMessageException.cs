using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventHandling
{
    public sealed class InvalidDomainMessageException : MessageHandlingException
    {
        public InvalidDomainMessageException(string message) : base(message)
        {
        }
    }
}
