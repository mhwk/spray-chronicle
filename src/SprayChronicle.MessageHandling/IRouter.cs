using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public interface IRouter<TTarget>
        where TTarget : class
    {
        IRouter<TTarget> Subscribe(IMessageHandlingStrategy<TTarget> strategy, HandleMessage handler);

        IRouter<TTarget> Subscribe(IRouterSubscriber<TTarget> subscriber);

        Task<object> Route(params object[] arguments);
    }
}
