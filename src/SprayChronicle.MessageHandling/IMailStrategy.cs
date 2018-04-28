using System;
using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public interface IMailStrategy
    {
        bool Resolves(string messageName);
        
        bool Resolves(object message);
        
        Type ToType(string messageName);
    }
    
    public interface IMailStrategy<in T> : IMailStrategy
    {
        Task Tell(T subject, object message, DateTime epoch);
        
        Task<TResult> Ask<TResult>(T subject, object message, DateTime epoch) where TResult : class;
    }
}
