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
        readonly ILogger<HttpCommandDispatcher> _logger;

        readonly IAuthorizer _authorizer;

        readonly IDispatchCommands _dispatcher;

        readonly Type _type;

        static readonly RequestToMessageConverter converter = new RequestToMessageConverter();

        public HttpCommandDispatcher(
            ILogger<HttpCommandDispatcher> logger,
            IAuthorizer authorizer,
            IDispatchCommands dispatcher,
            Type type)
        {
            _logger = logger;
            _authorizer = authorizer;
            _dispatcher = dispatcher;
            _type = type;
        }

        public async Task Dispatch(HttpContext context)
        {
            using (var reader = new StreamReader(context.Request.Body)) {
                context.Response.ContentType = "application/json";

                try {
                    _authorizer.Authorize(_type, context);

                    var payload = await converter.Convert(context.Request, _type);
                    _logger.LogDebug("Dispatching {0} {1}", _type, JsonConvert.SerializeObject(payload));
                    _dispatcher.Dispatch(payload);
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
                } catch (UnauthorizedException error) {
                    _logger.LogWarning(error.ToString());
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                        Error = error.Message
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
