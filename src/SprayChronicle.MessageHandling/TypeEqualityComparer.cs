using System;
using System.Collections.Generic;

namespace SprayChronicle.MessageHandling
{
    public class TypeEqualityComparer : IEqualityComparer<Type[]>
    {
        public bool Equals(Type[] x, Type[] y)
        {
            if (x.Length != y.Length) {
                return false;
            }
            for (int i = 0; i < x.Length; i++) {
                if ( ! x[i].Equals(y[i])) {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(Type[] obj)
        {
            unchecked {
                int hash = (int) 2166136261;
                foreach (var type in obj) {
                    hash = (hash * 16777619) ^ type.GetHashCode();
                }
                return hash;
            }
        }
    }
}