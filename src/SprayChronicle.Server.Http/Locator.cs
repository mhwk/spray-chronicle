using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SprayChronicle.Server.Http
{
    public sealed class Locator
    {
        public static IEnumerable<Type> LocateWithAttribute<T>()
        {
            return LocateWithAttribute(typeof(T));
        }

        public static IEnumerable<Type> LocateWithAttribute(Type type)
        {
            return Assembly.GetEntryAssembly().GetTypes().Where(candidate => candidate.GetCustomAttributes(type).Any());
        }
    }
}
