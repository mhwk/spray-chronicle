using System;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class SourceBuildException : OuroException
    {
        public SourceBuildException(string message) : base(message)
        {
        }
    }
}
