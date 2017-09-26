using System;

namespace SprayChronicle.Server.Http
{
    [AttributeUsageAttribute(AttributeTargets.Class)]
    public class HttpCommandAttribute : HttpAttribute
    {
        public readonly string Template;

        public HttpCommandAttribute(string template)
        {
            Template = template;
            Method = "POST";
            ContentType = "application/json";
        }
    }
}
