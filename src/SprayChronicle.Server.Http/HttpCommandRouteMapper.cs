using System.Reflection;
using Microsoft.AspNetCore.Routing;
using SprayChronicle.CommandHandling;

namespace SprayChronicle.Server.Http
{
    public class HttpCommandRouteMapper
    {
        private readonly ILogger<HttpCommandDispatcher> _logger;

        private readonly IValidator _validator;

        private readonly ICommandDispatcher _dispatcher;

        public HttpCommandRouteMapper(
            ILogger<HttpCommandDispatcher> logger,
            IValidator validator,
            ICommandDispatcher dispatcher)
        {
            _logger = logger;
            _validator = validator;
            _dispatcher = dispatcher;
        }

        public void Map(RouteBuilder builder)
        {
            foreach (var command in Locator.LocateWithAttribute<HttpCommandAttribute>()) {
                var template = command.GetTypeInfo().GetCustomAttribute<HttpCommandAttribute>()?.Template;
                
                _logger.LogDebug($"Mapping {template} to command {command}");
                switch (command.GetTypeInfo().GetCustomAttribute<HttpCommandAttribute>().Method) {
                    case "POST":
                        builder.MapPost(
                            command.GetTypeInfo().GetCustomAttribute<HttpCommandAttribute>().Template,
                            new HttpCommandDispatcher(_logger, _validator, _dispatcher, command).Dispatch
                        );
                    break;
                    case "GET":
                        builder.MapGet(
                            command.GetTypeInfo().GetCustomAttribute<HttpCommandAttribute>().Template,
                            new HttpCommandDispatcher(_logger, _validator, _dispatcher, command).Dispatch
                        );
                    break;
                    default: throw new UnsupportedHttpMethodException(string.Format(
                        "The http method {0} is not supported, must be either POST or GET",
                        command.GetTypeInfo().GetCustomAttribute<HttpCommandAttribute>().Method
                    ));
                }
            }
        }
    }
}
