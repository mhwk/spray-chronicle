using System;
using Microsoft.AspNetCore.Http;

namespace SprayChronicle.Server.Http
{
    public class VoidAuthorizer : IAuthorizer
    {
        public void Authorize(Type type, HttpContext httpContext)
        {}
    }
}