using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public abstract class Processed : EventHandling.Processed
    {
        public static Task<Processed> WithError(UnhandledCommandException error)
        {
            return Task.FromResult<Processed>(new ProcessedError(error));
        }
    }
}
