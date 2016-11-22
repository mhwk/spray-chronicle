using System;

namespace SprayChronicle.Persistence.Ouro
{
    public class OuroException : Exception
    {
        public OuroException(string message, Exception error): base(message, error)
        {}
    }
}