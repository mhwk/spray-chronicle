using System;

namespace SprayChronicle.Server.Http
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message): base(message)
        {}
    }
}