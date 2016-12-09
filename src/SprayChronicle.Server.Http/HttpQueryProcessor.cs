using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Server.Http
{
    public class HttpQueryProcessor
    {
        ILogger<HttpQueryProcessor> _logger;

        IProcessQueries _dispatcher;

        Type _type;

        public HttpQueryProcessor(ILogger<HttpQueryProcessor> logger, IProcessQueries dispatcher, Type type)
        {
            _logger = logger;
            _dispatcher = dispatcher;
            _type = type;
        }

        public async Task Process(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try {
                var payload = JsonConvert.SerializeObject(BuildObject(context.Request.Query));
                _logger.LogInformation("Processing {0} {1}", _type, payload);
                var result = _dispatcher.Process(JsonConvert.DeserializeObject(payload, _type));
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(result));
            } catch (UnhandledQueryException error) {
                _logger.LogError(error.ToString());
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                    Error = error.InnerException.Message
                }));
            } catch (Exception error) {
                _logger.LogCritical(error.ToString());
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                    Error = error.Message
                }));
            }
        }

        object BuildObject(IQueryCollection query)
        {
            var dict = new Dictionary<string,string>();
            foreach (var key in query.Keys) {
                StringValues @value = new StringValues();
                query.TryGetValue(key, out @value);
                dict.Add(key, @value.ToString());
            }
            return dict;
        }
    }
}