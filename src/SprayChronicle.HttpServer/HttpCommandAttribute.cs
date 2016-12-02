using System;

namespace SprayChronicle.HttpServer
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
