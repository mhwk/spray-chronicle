using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SprayChronicle.CommandHandling;

namespace SprayChronicle.Server.Http
{
    public class HttpCommandRouteMapper
    {
        readonly ILogger<HttpCommandDispatcher> _logger;

        readonly IDispatchCommands _dispatcher;

        public HttpCommandRouteMapper(
            ILogger<HttpCommandDispatcher> logger,
            IDispatchCommands dispatcher)
        {
            _logger = logger;
            _dispatcher = dispatcher;
        }

        public void Map(RouteBuilder builder)
        {
            foreach (var command in Locator.LocateWithAttribute<HttpCommandAttribute>()) {
                builder.MapPost(
                    command.GetTypeInfo().GetCustomAttribute<HttpCommandAttribute>().Template,
                    new HttpCommandDispatcher(_logger, _dispatcher, command).Dispatch
                );
            }
        }
    }
}
