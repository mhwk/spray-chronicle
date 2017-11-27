using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

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

        public MethodInfo[] MethodsForTypes(IEnumerable<Type> types)
        {
            if (null == types) {
                return new MethodInfo[] {};
            }
            
            var enumerable = types as Type[] ?? types.ToArray();
            
            if ( ! _map.ContainsKey(enumerable)) {
                _map[enumerable] = new List<MethodInfo>();
            }
            
            return _map[enumerable].ToArray();
        }

        public MethodInfo[] MethodsFor(IEnumerable<object> args)
        {
            return MethodsForTypes(args.Select(arg => arg.GetType()));
        }
    }
}
