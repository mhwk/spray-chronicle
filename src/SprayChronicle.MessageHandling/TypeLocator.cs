using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SprayChronicle.MessageHandling
{
    public abstract class TypeLocator : ILocateTypes
    {
        public IEnumerable<Type> LocateTypesWithAttribute<T>()
        {
            return LocateTypesWithAttribute(typeof(T));
        }

        public IEnumerable<Type> LocateTypesWithAttribute(Type type)
        {
            return LocateTargetTypes().Where(candidate => candidate.GetCustomAttributes(type).Any());
        }

        public IEnumerable<Type> LocateTypesWithParent<T>()
        {
            return LocateTypesWithParent(typeof(T));
        }

        public IEnumerable<Type> LocateTypesWithParent(Type type)
        {
            return LocateTargetTypes().Where(candidate =>
                candidate.IsInstanceOfType(type)
                || candidate.IsAssignableFrom(type)
                || candidate.IsSubclassOf(type)
            );
        }

        protected abstract IEnumerable<Type> LocateTargetTypes();
    }
}
