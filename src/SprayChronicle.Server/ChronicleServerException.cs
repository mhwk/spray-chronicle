using System;

namespace SprayChronicle.Server
{
    public abstract class ChronicleServerException : Exception
    {
        public ChronicleServerException(string message): base(message)
        {
        }
    }
}
