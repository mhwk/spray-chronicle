using System;

namespace SprayChronicle.CommandHandling
{
    public class ProcessedError : Processed
    {
        public Exception Exception { get; }

        public ProcessedError(Exception exception)
        {
            Exception = exception;
        }
    }
}