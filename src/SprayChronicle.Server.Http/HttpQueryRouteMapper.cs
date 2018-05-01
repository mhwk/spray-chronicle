using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Server.Http
{
    public class HttpQueryRouteMapper
    {
        private readonly ILogger<HttpQueryProcessor> _logger;
        
        private readonly IValidator _validator;

        private readonly IQueryDispatcher _dispatcher;

        private readonly List<IAttributeProvider<HttpQueryAttribute>> _queryProviders = new List<IAttributeProvider<HttpQueryAttribute>>();

        public HttpQueryRouteMapper(
            ILogger<HttpQueryProcessor> logger,
            IValidator validator,
            IQueryDispatcher dispatcher)
        {
            _logger = logger;
            _validator = validator;
            _dispatcher = dispatcher;
        }

        public void RegisterAttributeProvider(IAttributeProvider<HttpQueryAttribute> provider)
        {
            _queryProviders.Add(provider);
        }

        public void Map(RouteBuilder builder)
        {
            foreach (var provider in _queryProviders) {
                foreach (var kv in provider.Provide()) {
                    var metadata = kv.Key;
                    var query = kv.Value;
                    
                    _logger.LogDebug($"Mapping {metadata.Template} to query {query}");
                    switch (metadata.Method) {
                        case "POST":
                            builder.MapPost(
                                metadata.Template,
                                new HttpQueryProcessor(_logger, _validator, _dispatcher, query, metadata.ContentType).Process
                            );
                            break;
                        case "GET":
                            builder.MapGet(
                                metadata.Template,
                                new HttpQueryProcessor(_logger, _validator, _dispatcher, query, metadata.ContentType).Process
                            );
                            break;
                        default:
                            throw new UnsupportedHttpMethodException(
                                $"Unable to map route {metadata.Template} to {query.Name}, expected method GET or POST but was {metadata.Method}"
                            );
                    }
                }
            }
        }
    }
}
