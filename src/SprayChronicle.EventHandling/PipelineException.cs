using System;

namespace SprayChronicle.EventHandling
{
    public sealed class PipelineException : Exception
    {
        public PipelineException(string message): base(message)
        {
        }
    }
}
