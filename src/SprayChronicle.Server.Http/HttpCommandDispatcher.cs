using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Server.Http
{
    public class HttpCommandDispatcher
    {
        ILogger<HttpCommandDispatcher> _logger;

        IDispatchCommands _dispatcher;

        Type _type;

        public HttpCommandDispatcher(ILogger<HttpCommandDispatcher> logger, IDispatchCommands dispatcher, Type type)
        {
            _logger = logger;
            _dispatcher = dispatcher;
            _type = type;
        }

        public async Task Dispatch(HttpContext context)
        {
            using (var reader = new StreamReader(context.Request.Body)) {
                context.Response.ContentType = "application/json";

                try {
                    var payload = await reader.ReadToEndAsync();
                    _logger.LogInformation("Dispatching {0} {1}", _type, payload);
                    _dispatcher.Dispatch(JsonConvert.DeserializeObject(payload, _type));
                } catch (UnhandledCommandException error) {
                    if (error.InnerException is ConcurrencyException) {
                        _logger.LogWarning(error.ToString());
                        context.Response.StatusCode = 409;
                    } else if (error.InnerException is InvalidStateException) {
                        _logger.LogWarning(error.ToString());
                        context.Response.StatusCode = 428;
                    } else {
                        _logger.LogError(error.ToString());
                        context.Response.StatusCode = 500;
                    }
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
}
