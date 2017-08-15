using System;

namespace SprayChronicle.MessageHandling
{
    public abstract class MessageHandlingException : Exception
    {
        protected MessageHandlingException(string message): base(message)
        {}
    }
}