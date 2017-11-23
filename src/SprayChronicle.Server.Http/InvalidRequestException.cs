using System;

namespace SprayChronicle.Server.Http
{
    public sealed class InvalidRequestException : HttpServerException
    {
        public InvalidRequestException(string message): base(message)
        {}

        public InvalidRequestException(string message, Exception inner): base(message, inner)
        {}
    }
}
