using System;
using System.Collections.Generic;

namespace SprayChronicle.MessageHandling
{
    public interface ILocateTypes
    {
        IEnumerable<Type> LocateTypesWithAttribute<T>();

        IEnumerable<Type> LocateTypesWithAttribute(Type type);

        IEnumerable<Type> LocateTypesWithParent<T>();
        
        IEnumerable<Type> LocateTypesWithParent(Type type);
    }
}
