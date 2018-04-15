using System;

namespace SprayChronicle.Persistence.Ouro
{
    public abstract class OuroException : Exception
    {
        public OuroException(string message): base(message)
        {}
    }
}