using System;

namespace SprayChronicle.Server.Http
{
    [AttributeUsageAttribute(AttributeTargets.Class)]
    public class HttpAttribute : Attribute
    {
        public string Method { set; get; }
    }
}