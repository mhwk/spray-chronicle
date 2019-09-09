using System.Threading.Tasks;

namespace SprayChronicle
{
    public sealed class CommandDispatcher
    {
        private readonly IStoreEvents _events;
        private readonly IStoreSnapshots _snapshots;
        private readonly object _command;
        
        public delegate CommandDispatcher Factory(object command);

        public CommandDispatcher(
            IStoreEvents events,
            IStoreSnapshots snapshots,
            object command
        )
        {
            _events = events;
            _snapshots = snapshots;
            _command = command;
        }
        
        public Task To<TInvariant>(string invariantId, string messageId = null)
            where TInvariant : IArrange<TInvariant>, IAct<TInvariant>
        {
            return new CommandHandler<TInvariant>(
                _events,
                _snapshots,
                invariantId
            ).Handle(_command, messageId);
        }
    }
}
