namespace SprayChronicle.MessageHandling
{
    public interface IRouterSubscriber<TTarget>
        where TTarget : class
    {
        void Subscribe(IRouter<TTarget> router);
    }
}
