using System;

namespace SprayChronicle.Server.Http
{
    public abstract class HttpServerException : Exception
    {
        protected HttpServerException(string message) : base(message)
        {
        }
        
        protected HttpServerException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}