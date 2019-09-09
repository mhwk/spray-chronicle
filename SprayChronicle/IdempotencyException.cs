using System;

namespace SprayChronicle
{
    public class IdempotencyException : ArgumentException
    {
        public IdempotencyException(string messageId) : base($"Message with id {messageId} already handled")
        {
        }
    }
}
