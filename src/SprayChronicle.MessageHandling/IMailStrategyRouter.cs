namespace SprayChronicle.MessageHandling
{
    public interface IMailStrategyRouter<TTarget>
        where TTarget : class
    {
        IMailStrategyRouter<TTarget> Subscribe(IMailStrategy strategy, MailHandler handler);

        IMailStrategyRouter<TTarget> Subscribe(IMailStrategyRouterSubscriber<TTarget> subscriber);
    }
}