using System;

namespace SprayChronicle.MessageHandling
{
    public abstract class MessageHandlingException : Exception
    {
        public MessageHandlingException(string message): base(message)
        {}
    }
}