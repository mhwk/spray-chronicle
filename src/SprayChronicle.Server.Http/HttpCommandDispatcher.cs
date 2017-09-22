using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Server.Http
{
    public class HttpCommandDispatcher
    {
        readonly ILogger<HttpCommandDispatcher> _logger;

        readonly IAuthorizer _authorizer;

        readonly IValidator _validator;

        readonly IDispatchCommand _dispatcher;

        readonly Type _type;

        readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        static readonly RequestToMessageConverter converter = new RequestToMessageConverter();

        public HttpCommandDispatcher(
            ILogger<HttpCommandDispatcher> logger,
            IAuthorizer authorizer,
            IValidator validator,
            IDispatchCommand dispatcher,
            Type type)
        {
            _logger = logger;
            _authorizer = authorizer;
            _validator = validator;
            _dispatcher = dispatcher;
            _type = type;
        }

        public async Task Dispatch(HttpContext context)
        {
            using (var reader = new StreamReader(context.Request.Body)) {
                context.Response.ContentType = "application/json";

                try {
                    var payload = await converter.Convert(context.Request, _type);

                    _authorizer.Authorize(payload, context);
                    _validator.Validate(payload);

                    _logger.LogDebug("Dispatching {0} {1}", _type, JsonConvert.SerializeObject(payload));
                    _dispatcher.Dispatch(payload);
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                        Success = true
                    }, _serializerSettings));
                } catch (UnhandledCommandException error) {
                    _logger.LogWarning(error.ToString());
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                        Success = false,
                        Error = error.Message
                    }, _serializerSettings));
                } catch (ConcurrencyException error) {
                    _logger.LogWarning(error.ToString());
                    context.Response.StatusCode = 409;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                        Success = false,
                        Error = error.Message
                    }, _serializerSettings));
                } catch (InvalidStateException error) {
                    _logger.LogWarning(error.ToString());
                    context.Response.StatusCode = 428;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                        Success = false,
                        Error = error.Message
                    }, _serializerSettings));
                } catch (UnauthorizedException error) {
                    _logger.LogWarning(error.ToString());
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                        Success = false,
                        Error = error.Message
                    }, _serializerSettings));
                } catch (InvalidatedException error) {
                    _logger.LogWarning(error.ToString());
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                        Success = false,
                        Error = error.Message
                    }, _serializerSettings));
                } catch (Exception error) {
                    _logger.LogCritical(error.ToString());
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                        Success = false,
                        Error = error.Message
                    }, _serializerSettings));
                }
            }
        }
    }
}
