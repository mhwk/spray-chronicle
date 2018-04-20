using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace SprayChronicle.MessageHandling
{
    public class MethodsForTypeDictionary
    {
        private readonly Dictionary<Type[],List<MethodInfo>> _map  = new Dictionary<Type[],List<MethodInfo>>(new CompareTypeEquality());

        public void Add(Type[] type, MethodInfo methodInfo)
        {
            if ( ! _map.ContainsKey(type)) {
                _map[type] = new List<MethodInfo>();
            }
            
            _map[type].Add(methodInfo);
        }

        public MethodInfo[] MethodsFor(IEnumerable<Type> types)
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
            if (args.Any(a => a == null)) {
                throw new ArgumentException($"Null value not allowed in args");
            }
            
            return MethodsFor(args.Select(arg => arg.GetType()));
        }

        public IEnumerable<string> SuggestList(object subject)
        {
            return _map
//                .Where(kv => !kv.Value.All(t => t.IsStatic && t.DeclaringType.IsInstanceOfType(subject)))
                .Select(kv => kv.Key)
                .Select(ts => string.Join(", ", ts.Select(t => t.FullName)))
                .ToList();
        }
    }
}
