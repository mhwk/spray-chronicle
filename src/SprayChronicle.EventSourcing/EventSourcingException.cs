using System;

namespace SprayChronicle.EventSourcing
{
    public class EventSourcingException : Exception
    {
        public EventSourcingException(string message): base(message)
        {}

        public EventSourcingException(string message, Exception error): base(message, error)
        {}
    }
}