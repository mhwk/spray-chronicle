using System;

namespace SprayChronicle
{
    public interface IMapState
    {
        IMapState Map<T>();
        IMapState Map<T>(string name);
        IMapState Map<T>(string name, Func<IServiceProvider, object> creator);
        IMapState Map<T>(Func<IServiceProvider, object> creator);
    }
}
