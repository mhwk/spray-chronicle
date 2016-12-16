namespace SprayChronicle.EventHandling
{
    public sealed class AsyncEventHandlingModule : EventHandlingModule
    {
        protected override IManageStreamHandlers CreateManager()
        {
            return new AsyncStreamHandlerManager();
        }
    }
}
