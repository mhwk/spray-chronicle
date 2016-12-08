using System;

namespace SprayChronicle.Server.Http
{
    [AttributeUsageAttribute(AttributeTargets.Class)]
    public class HttpQueryAttribute : Attribute
    {
        public readonly string Template;

        public HttpQueryAttribute(string template)
        {
            Template = template;
        }
    }
}
