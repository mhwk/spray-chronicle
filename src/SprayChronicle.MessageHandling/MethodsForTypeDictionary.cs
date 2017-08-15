using System;
using System.Reflection;
using System.Collections.Generic;

namespace SprayChronicle.MessageHandling
{
    public class MethodsForTypeDictionary
    {
        private readonly Dictionary<Type[],List<MethodInfo>> _map  = new Dictionary<Type[],List<MethodInfo>>(new TypeEqualityComparer());

        public void Add(Type[] type, MethodInfo methodInfo)
        {
            if ( ! _map.ContainsKey(type)) {
                _map[type] = new List<MethodInfo>();
            }
            _map[type].Add(methodInfo);
        }

        public IEnumerable<MethodInfo> MethodsFor(Type[] type)
        {
            if ( ! _map.ContainsKey(type)) {
                _map[type] = new List<MethodInfo>();
            }
            return _map[type];
        }
    }
}