using System;

namespace SprayChronicle.Server.Http
{
    public class InvalidatedException : Exception
    {
        public InvalidatedException(string message): base(message)
        {}

        public InvalidatedException(string message, Exception inner): base(message, inner)
        {}
    }
}
