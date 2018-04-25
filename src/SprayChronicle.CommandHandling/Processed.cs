using System;
using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public abstract class Processed : EventHandling.Processed
    {
        public static Task<Processed> WithError(Exception error)
        {
            return Task.FromResult<Processed>(new ProcessedError(error));
        }
    }
}
