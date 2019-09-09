namespace SprayChronicle
{
    public class CommandDispatcherFactory
    {
        private readonly IStoreEvents _events;
        private readonly IStoreSnapshots _snapshots;

        public CommandDispatcherFactory(
            IStoreEvents events,
            IStoreSnapshots snapshots
        )
        {
            _events = events;
            _snapshots = snapshots;
        }

        public CommandDispatcher Build(object command)
        {
            return new CommandDispatcher(_events, _snapshots, command);
        }
    }
}
