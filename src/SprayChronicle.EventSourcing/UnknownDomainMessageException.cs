using System;

namespace SprayChronicle.EventSourcing
{
    public class UnknownDomainMessageException : EventSourcingException
    {
        public UnknownDomainMessageException(string message): base(message)
        {}

        public UnknownDomainMessageException(string message, Exception error): base(message, error)
        {}
    }
}