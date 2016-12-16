namespace SprayChronicle.EventHandling
{
    public sealed class SyncEventHandlingModule : EventHandlingModule
    {
        protected override IManageStreamHandlers CreateManager()
        {
            return new SyncStreamHandlerManager();
        }
    }
}