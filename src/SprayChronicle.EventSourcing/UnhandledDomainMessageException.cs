using System;

namespace SprayChronicle.EventSourcing
{
    public class UnhandledDomainMessageException : EventSourcingException
    {
        public UnhandledDomainMessageException(string message): base(message)
        {}

        public UnhandledDomainMessageException(string message, Exception error): base(message, error)
        {}
    }
}