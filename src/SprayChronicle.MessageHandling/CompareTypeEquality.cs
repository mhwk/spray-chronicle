using System;
using System.Collections.Generic;
using System.Linq;

namespace SprayChronicle.MessageHandling
{
    internal sealed class CompareTypeEquality : IEqualityComparer<Type[]>
    {
        public bool Equals(Type[] x, Type[] y)
        {
            if (null == x || null == y) {
                throw new ArgumentException($"X or Y can not be empty");
            }
            if (x.Length != y.Length) {
                return false;
            }
            return !x.Where((t, i) => t != y[i]).Any();
        }

        public int GetHashCode(Type[] obj)
        {
            unchecked
            {
                return obj.Aggregate((int) 2166136261, (current, type) => (current * 16777619) ^ type.GetHashCode());
            }
        }
    }
}