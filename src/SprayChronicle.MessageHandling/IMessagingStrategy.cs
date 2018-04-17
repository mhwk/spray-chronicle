using System;
using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public interface IMessagingStrategy<in T>
    {
        bool Resolves(string messageName);
        
        bool Resolves(params Type[] arguments);
        
        bool Resolves(params object[] arguments);
        
        bool Resolves<TResult>(params Type[] arguments);
        
        bool Resolves<TResult>(params object[] arguments);
        
        Task Tell(T subject, params object[] arguments);
        
        Task<TResult> Ask<TResult>(T subject, params object[] arguments) where TResult : class;
        
        Type ToType(string messageName);
    }
}
