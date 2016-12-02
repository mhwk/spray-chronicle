using System;

namespace SprayChronicle.Server.Http
{
    [AttributeUsageAttribute(AttributeTargets.Class)]
    public class HttpCommandAttribute : Attribute
    {
        public readonly string Template;

        public HttpCommandAttribute(string template)
        {
            Template = template;
        }
    }
}
