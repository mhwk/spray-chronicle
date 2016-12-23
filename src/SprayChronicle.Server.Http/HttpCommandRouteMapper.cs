using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SprayChronicle.CommandHandling;

namespace SprayChronicle.Server.Http
{
    public class HttpCommandRouteMapper
    {
        readonly ILogger<HttpCommandDispatcher> _logger;

        readonly IAuthorizer _authorizer;

        readonly IValidator _validator;

        readonly IDispatchCommands _dispatcher;

        public HttpCommandRouteMapper(
            ILogger<HttpCommandDispatcher> logger,
            IAuthorizer authorizer,
            IValidator validator,
            IDispatchCommands dispatcher)
        {
            _logger = logger;
            _authorizer = authorizer;
            _validator = validator;
            _dispatcher = dispatcher;
        }

        public void Map(RouteBuilder builder)
        {
            foreach (var command in Locator.LocateWithAttribute<HttpCommandAttribute>()) {
                _logger.LogDebug("Mapping {0} to command {1}", command.GetTypeInfo().GetCustomAttribute<HttpCommandAttribute>().Template, command);
                switch (command.GetTypeInfo().GetCustomAttribute<HttpCommandAttribute>().Method) {
                    case "POST":
                        builder.MapPost(
                            command.GetTypeInfo().GetCustomAttribute<HttpCommandAttribute>().Template,
                            new HttpCommandDispatcher(_logger, _authorizer, _validator, _dispatcher, command).Dispatch
                        );
                    break;
                    case "GET":
                        builder.MapGet(
                            command.GetTypeInfo().GetCustomAttribute<HttpCommandAttribute>().Template,
                            new HttpCommandDispatcher(_logger, _authorizer, _validator, _dispatcher, command).Dispatch
                        );
                    break;
                }
            }
        }
    }
}
