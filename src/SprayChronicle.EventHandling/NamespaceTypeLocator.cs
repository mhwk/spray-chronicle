using System;
using System.Linq;
using System.Collections.Generic;
using SprayLocator;

namespace SprayChronicle.EventHandling
{
    public class NamespaceTypeLocator : ILocateTypes
    {
        Dictionary<string,Type> _map = new Dictionary<string,Type>();

        public NamespaceTypeLocator(string @namespace)
        {
            foreach (var type in TypeLocator.LocateRuntimeTypes()
                .Where(t => t.Namespace == @namespace)) {
                _map.Add(type.Name, type);
            }
        }

        public Type Locate(string type)
        {
            if ( ! _map.ContainsKey(type)) {
                return default(Type);
            }
            return _map[type];
        }
    }
}
