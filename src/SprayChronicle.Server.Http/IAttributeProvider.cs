using System;
using System.Collections.Generic;

namespace SprayChronicle.Server.Http
{
    public interface IAttributeProvider<TAttribute>
        where TAttribute : Attribute
    {
        IEnumerable<KeyValuePair<TAttribute,Type>> Provide();
    }
}
