using System;
using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public interface IMessageHandlingStrategy<in T>
    {
        bool Resolves(T subject, params Type[] arguments);
        
        Task Tell(T subject, params object[] arguments);
        
        bool Resolves<TResult>(T subject, params Type[] arguments);
        
        Task<TResult> Ask<TResult>(T subject, params object[] arguments) where TResult : class;
        
        void EachType(Action<Type> action);
    }
}
