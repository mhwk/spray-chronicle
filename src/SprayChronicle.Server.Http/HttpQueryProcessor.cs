using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Server.Http
{
    public class HttpQueryProcessor
    {
        readonly ILogger<HttpQueryProcessor> _logger;

        readonly IProcessQueries _dispatcher;

        readonly Type _type;

        static readonly RequestToMessageConverter converter = new RequestToMessageConverter();

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
                var payload = await converter.Convert(context.Request, _type);
                _logger.LogDebug("Processing {0} {1}", _type, JsonConvert.SerializeObject(payload));
                var result = _dispatcher.Process(payload);
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

    }
}