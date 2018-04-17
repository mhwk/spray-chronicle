
namespace SprayChronicle.CommandHandling
{
    public sealed class ProcessedDispatch : Processed
    {
        public object Command { get; }

        public ProcessedDispatch(object command)
        {
            Command = command;
        }
    }
}