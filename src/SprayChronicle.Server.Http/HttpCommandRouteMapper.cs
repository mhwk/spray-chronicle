using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using SprayChronicle.CommandHandling;

namespace SprayChronicle.Server.Http
{
    public class HttpCommandRouteMapper
    {
        private readonly ILogger<HttpCommandDispatcher> _logger;

        private readonly IValidator _validator;

        private readonly ICommandDispatcher _dispatcher;
        
        private readonly List<IAttributeProvider<HttpCommandAttribute>> _commandProviders = new List<IAttributeProvider<HttpCommandAttribute>>();

        public HttpCommandRouteMapper(
            ILogger<HttpCommandDispatcher> logger,
            IValidator validator,
            ICommandDispatcher dispatcher)
        {
            _logger = logger;
            _validator = validator;
            _dispatcher = dispatcher;
        }

        public void RegisterAttributeProvider(IAttributeProvider<HttpCommandAttribute> provider)
        {
            _commandProviders.Add(provider);
        }

        public void Map(RouteBuilder builder)
        {
            var mapped = new List<Tuple<string, string, string>>();
            
            foreach (var provider in _commandProviders) {
                foreach (var kv in provider.Provide()) {
                    var metadata = kv.Key;
                    var command = kv.Value;

                    switch (metadata.Method) {
                        case "POST":
                            builder.MapPost(
                                metadata.Template,
                                new HttpCommandDispatcher(_logger, _validator, _dispatcher, command).Dispatch
                            );
                            break;
                        case "GET":
                            builder.MapGet(
                                metadata.Template,
                                new HttpCommandDispatcher(_logger, _validator, _dispatcher, command).Dispatch
                            );
                            break;
                        default:
                            throw new UnsupportedHttpMethodException(
                                $"Unable to map route {metadata.Template} to {command.Name}, expected method GET or POST but was {metadata.Method}"
                            );
                    }
                    
                    mapped.Add(new Tuple<string, string, string>(
                        metadata.Method,
                        metadata.Template,
                        command.Name
                    ));
                }
            }

            var mappedList = string.Join("\n", mapped.Select(m => $"  [{m.Item1}] {m.Item2} -> {m.Item3}"));
            _logger.LogDebug($"Mapped {mapped.Count} commands:\n{mappedList}");
        }
    }
}
