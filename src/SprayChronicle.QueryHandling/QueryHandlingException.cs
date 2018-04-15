using System;

namespace SprayChronicle.QueryHandling
{
    public abstract class QueryHandlingException : Exception
    {
        protected QueryHandlingException(string message) : base(message)
        {}
    }
}