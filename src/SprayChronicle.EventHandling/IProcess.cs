using System;
using System.Threading.Tasks;

namespace SprayChronicle.EventHandling
{
    public interface IProcess
    {
        
    }
    
    public interface IProcess<in T> : IProcess
    {
        Task<Processed> Process(T payload, DateTime epoch);
    }
}
