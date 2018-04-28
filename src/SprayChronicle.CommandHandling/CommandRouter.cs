using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public sealed class CommandRouter : MailStrategyRouter<IHandle>
    {
        public CommandRouter() : base(1)
        {
        }
    }
}
