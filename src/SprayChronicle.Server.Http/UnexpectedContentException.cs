using System;

namespace SprayChronicle.Server.Http
{
    public class UnexpectedContentException : Exception
    {
        public UnexpectedContentException(string message): base(message)
        {}
    }
}