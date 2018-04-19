using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Server.Http
{
    public class HttpQueryRouteMapper
    {
        readonly ILogger<HttpQueryProcessor> _logger;

        readonly IValidator _validator;

        readonly QueryRouter _executor;

        public HttpQueryRouteMapper(
            ILogger<HttpQueryProcessor> logger,
            IValidator validator,
            QueryRouter executor)
        {
            _logger = logger;
            _validator = validator;
            _executor = executor;
        }

        public void Map(RouteBuilder builder)
        {
            foreach (var query in Locator.LocateWithAttribute<HttpQueryAttribute>()) {
                _logger.LogDebug("Mapping {0} to query {1}", query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().Template, query);
                switch (query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().Method) {
                    case "POST":
                        builder.MapPost(
                            query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().Template,
                            new HttpQueryProcessor(_logger, _validator, _executor, query, query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().ContentType).Process
                        );
                    break;
                    case "GET":
                        builder.MapGet(
                            query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().Template,
                            new HttpQueryProcessor(_logger, _validator, _executor, query, query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().ContentType).Process
                        );
                    break;
                }
            }
        }
    }
}
