using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Server.Http
{
    public class HttpQueryRouteMapper
    {
        readonly ILogger<HttpQueryProcessor> _logger;

        readonly IProcessQueries _processor;

        public HttpQueryRouteMapper(
            ILogger<HttpQueryProcessor> logger,
            IProcessQueries processor)
        {
            _logger = logger;
            _processor = processor;
        }

        public void Map(RouteBuilder builder)
        {
            foreach (var query in Locator.LocateWithAttribute<HttpQueryAttribute>()) {
                _logger.LogDebug("Mapping {0} to query {1}", query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().Template, query);
                switch (query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().Method) {
                    case "POST":
                        builder.MapPost(
                            query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().Template,
                            new HttpQueryProcessor(_logger, _processor, query).Process
                        );
                    break;
                    case "GET":
                        builder.MapGet(
                            query.GetTypeInfo().GetCustomAttribute<HttpQueryAttribute>().Template,
                            new HttpQueryProcessor(_logger, _processor, query).Process
                        );
                    break;
                }
            }
        }
    }
}
