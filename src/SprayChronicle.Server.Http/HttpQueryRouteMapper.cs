using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Server.Http
{
    public class HttpQueryRouteMapper
    {
        private readonly ILogger<HttpQueryProcessor> _logger;

        private readonly IValidator _validator;

        private readonly IQueryDispatcher _dispatcher;

        public HttpQueryRouteMapper(
            ILogger<HttpQueryProcessor> logger,
            IValidator validator,
            IQueryDispatcher dispatcher)
        {
            _logger = logger;
            _validator = validator;
            _dispatcher = dispatcher;
        }

        public void Map(RouteBuilder builder)
        {
            foreach (var query in Locator.LocateWithAttribute<HttpQueryAttribute>()) {
                var template = query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().Template;
                _logger.LogDebug($"Mapping {template} to query {query}");
                switch (query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().Method) {
                    case "POST":
                        builder.MapPost(
                            query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().Template,
                            new HttpQueryProcessor(_logger, _validator, _dispatcher, query, query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().ContentType).Process
                        );
                    break;
                    case "GET":
                        builder.MapGet(
                            query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().Template,
                            new HttpQueryProcessor(_logger, _validator, _dispatcher, query, query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().ContentType).Process
                        );
                    break;
                }
            }
        }
    }
}
