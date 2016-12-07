using System;

namespace SprayChronicle.QueryHandling
{
    public class QueryHandlingException : Exception
    {
        public QueryHandlingException(string message) : base(message)
        {}
    }
}