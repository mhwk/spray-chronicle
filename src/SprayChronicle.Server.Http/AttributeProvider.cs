using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SprayChronicle.Server.Http
{
    public abstract class AttributeProvider<TAttribute> : IAttributeProvider<TAttribute>
        where TAttribute : Attribute
    {
        private readonly Assembly _assembly;

        protected AttributeProvider(Assembly assembly)
        {
            _assembly = assembly;
        }

        public IEnumerable<KeyValuePair<TAttribute,Type>> Provide()
        {
            return _assembly
                .GetTypes()
                .Where(candidate => candidate.GetCustomAttributes(typeof(TAttribute), false).Any())
                .SelectMany(candidate =>
                    candidate.GetCustomAttributes(typeof(TAttribute), false)
                        .Cast<TAttribute>()
                        .Select(attribute => new KeyValuePair<TAttribute, Type>(
                            attribute,
                            candidate
                        )
                    )
                );
        }
    }

}