using System;
using System.Collections.Generic;
using System.Linq;
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
            var mapped = new List<Tuple<string, string, string>>();
            
            foreach (var provider in _queryProviders) {
                foreach (var kv in provider.Provide()) {
                    var metadata = kv.Key;
                    var query = kv.Value;
                    
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
                    
                    mapped.Add(new Tuple<string, string, string>(
                        metadata.Method,
                        metadata.Template,
                        query.Name
                    ));
                }
            }
            
            var mappedList = string.Join("\n", mapped.Select(m => $"  [{m.Item1}] {m.Item2} -> {m.Item3}"));
            _logger.LogDebug($"Mapped {mapped.Count} queries:\n{mappedList}");
        }
    }
}
