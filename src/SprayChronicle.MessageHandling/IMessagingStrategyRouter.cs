namespace SprayChronicle.MessageHandling
{
    public interface IMessagingStrategyRouter<TTarget>
        where TTarget : class
    {
        IMessagingStrategyRouter<TTarget> Subscribe(IMessagingStrategy<TTarget> strategy, HandleMessage handler);

        IMessagingStrategyRouter<TTarget> Subscribe(IMessagingStrategyRouterSubscriber<TTarget> subscriber);
    }
}