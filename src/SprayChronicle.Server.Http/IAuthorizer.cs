using System;
using Microsoft.AspNetCore.Http;

namespace SprayChronicle.Server.Http
{
    public interface IAuthorizer
    {
        void Authorize(Type type, HttpContext httpContext);
    }
}
