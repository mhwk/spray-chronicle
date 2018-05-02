using System;
using System.Reflection;

namespace SprayChronicle.Server.Http
{
    public sealed class EntryAttributeProvider<TAttribute> : AttributeProvider<TAttribute>
        where TAttribute : Attribute
    {
        public EntryAttributeProvider() : base(Assembly.GetEntryAssembly())
        {
        }
    }
}
