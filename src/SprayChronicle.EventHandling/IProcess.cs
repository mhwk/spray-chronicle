using System;
using System.Threading.Tasks;

namespace SprayChronicle.EventHandling
{
    public interface IProcess
    {
        
    }
    
    public interface IProcess<in T> : IProcess
    {
        Task<EventProcessed> Process(T payload, DateTime epoch);
    }
}
