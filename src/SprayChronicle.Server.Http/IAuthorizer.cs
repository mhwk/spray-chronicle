using Microsoft.AspNetCore.Http;

namespace SprayChronicle.Server.Http
{
    public interface IAuthorizer
    {
        void Authorize(object payload, HttpContext httpContext);
    }
}
