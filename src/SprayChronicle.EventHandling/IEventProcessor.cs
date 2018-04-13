using System;
using System.Threading.Tasks;

namespace SprayChronicle.EventHandling
{
    public interface IEventProcessor
    {
        
    }
    
    public interface IEventProcessor<in T> : IEventProcessor
    {
        Task Process(T payload, DateTime epoch);
    }
}
