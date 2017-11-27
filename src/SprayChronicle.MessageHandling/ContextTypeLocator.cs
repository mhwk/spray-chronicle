using System;
using System.Collections.Generic;

namespace SprayChronicle.MessageHandling
{
    public sealed class ContextTypeLocator<T> : TypeLocator
    {
        protected override IEnumerable<Type> LocateTargetTypes()
        {
            return typeof(T).Assembly.GetTypes();
        }
    }
}
