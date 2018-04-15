using System;
using System.Reflection;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class Metadata
    {
        public readonly string OriginalFqn;

        public Metadata(Type originalFqn)
        {
            OriginalFqn = string.Format(
                "{0}, {1}",
                originalFqn.ToString(),
                originalFqn.GetTypeInfo().Assembly
            );
        }
    }
}
