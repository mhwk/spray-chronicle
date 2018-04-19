using System;

namespace SprayChronicle.CommandHandling
{
    public class UnhandledCommandException : Exception
    {
        public UnhandledCommandException(string message): base(message)
        {}
    }
}
