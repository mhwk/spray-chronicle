using System;
using System.Threading.Tasks;

namespace SprayChronicle.EventHandling
{
    public interface IProcessEvents
    {
        Task Process(object @event, DateTime at);
    }
}
