using System;

namespace SprayChronicle.Server.Http
{
    public sealed class UnexpectedContentException : HttpServerException
    {
        public UnexpectedContentException(string message) : base(message)
        {
        }
    }
}
