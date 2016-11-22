using System.Collections.Generic;

namespace SprayChronicle.EventHandling
{
    public interface IManageStreamHandlers
    {
        void Add(IEnumerable<IHandleStream> handlers);

        void Add(IHandleStream handler);

        void Manage();
    }
}
