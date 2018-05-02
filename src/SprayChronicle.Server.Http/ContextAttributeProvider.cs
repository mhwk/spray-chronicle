using System;

namespace SprayChronicle.Server.Http
{
    public sealed class ContextAttributeProvider<TContext, TAttribute> : AttributeProvider<TAttribute>
        where TAttribute : Attribute
    {
        public ContextAttributeProvider() : base(typeof(TContext).Assembly)
        {
        }
    }
}
