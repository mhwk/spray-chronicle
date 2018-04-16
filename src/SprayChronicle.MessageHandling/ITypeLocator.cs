using System;
using System.Collections.Generic;

namespace SprayChronicle.MessageHandling
{
    public interface ITypeLocator
    {
        IEnumerable<Type> LocateTypesWithAttribute<T>();

        IEnumerable<Type> LocateTypesWithAttribute(Type type);

        IEnumerable<Type> LocateTypesWithParent<T>();
        
        IEnumerable<Type> LocateTypesWithParent(Type type);
    }
}
