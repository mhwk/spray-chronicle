namespace SprayChronicle.MessageHandling
{
    public interface IMessagingStrategyRouter<TTarget>
        where TTarget : class
    {
        IMessagingStrategyRouter<TTarget> Subscribe(IMessagingStrategy strategy, HandleMessage handler);

        IMessagingStrategyRouter<TTarget> Subscribe(IMessagingStrategyRouterSubscriber<TTarget> subscriber);
    }
}