using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public interface IMessageHandlingStrategy<T>
    {
        Task Tell(T subject, IMessage message, params object[] arguments);
        
        Task<TResult> Ask<TResult>(T subject, IMessage message, params object[] arguments) where TResult : class;
    }
}
