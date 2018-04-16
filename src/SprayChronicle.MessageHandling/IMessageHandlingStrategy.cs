using System;
using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public interface IMessageHandlingStrategy<in T>
    {
        bool Resolves(params Type[] arguments);
        
        bool Resolves(params object[] arguments);
        
        bool Resolves<TResult>(params Type[] arguments);
        
        bool Resolves<TResult>(params object[] arguments);
        
        Task Tell(T subject, params object[] arguments);
        
        Task<TResult> Ask<TResult>(T subject, params object[] arguments) where TResult : class;
    }
}
