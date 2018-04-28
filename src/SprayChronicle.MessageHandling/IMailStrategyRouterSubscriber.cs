namespace SprayChronicle.MessageHandling
{
    public interface IMailStrategyRouterSubscriber<TTarget>
        where TTarget : class
    {
        void Subscribe(IMailStrategyRouter<TTarget> messageRouter);
    }
}
