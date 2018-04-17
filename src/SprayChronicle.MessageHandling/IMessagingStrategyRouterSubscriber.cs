namespace SprayChronicle.MessageHandling
{
    public interface IMessagingStrategyRouterSubscriber<TTarget>
        where TTarget : class
    {
        void Subscribe(IMessagingStrategyRouter<TTarget> messageRouter);
    }
}
