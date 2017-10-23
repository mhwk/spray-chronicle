using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Server.Http
{
    public class HttpCommandDispatcher
    {
        readonly ILogger<HttpCommandDispatcher> _logger;

        readonly IValidator _validator;

        readonly IDispatchCommand _dispatcher;

        readonly Type _type;

        readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>() { new ISO8601DateConverter() }
        };

        static readonly RequestToMessageConverter converter = new RequestToMessageConverter();

        public HttpCommandDispatcher(
            ILogger<HttpCommandDispatcher> logger,
            IValidator validator,
            IDispatchCommand dispatcher,
            Type type)
        {
            _logger = logger;
            _validator = validator;
            _dispatcher = dispatcher;
            _type = type;
        }

        public async Task Dispatch(HttpRequest request, HttpResponse response, RouteData routeData)
        {
            using (var reader = new StreamReader(request.Body)) {
                response.ContentType = "application/json";

                try {
                    var payload = await converter.Convert(request, routeData, _type);

                    _validator.Validate(payload);

                    _logger.LogDebug("Dispatching {0} {1}", _type, JsonConvert.SerializeObject(payload));
                    _dispatcher.Dispatch(payload);
                    response.StatusCode = 200;
                    await response.WriteAsync(JsonConvert.SerializeObject(new {
                        Success = true
                    }, _serializerSettings));
                } catch (UnhandledCommandException error) {
                    _logger.LogWarning(error.ToString());
                    response.StatusCode = 404;
                    await response.WriteAsync(JsonConvert.SerializeObject(new {
                        Success = false,
                        Error = error.Message
                    }, _serializerSettings));
                } catch (ConcurrencyException error) {
                    _logger.LogWarning(error.ToString());
                    response.StatusCode = 409;
                    await response.WriteAsync(JsonConvert.SerializeObject(new {
                        Success = false,
                        Error = error.Message
                    }, _serializerSettings));
                } catch (InvalidStateException error) {
                    _logger.LogWarning(error.ToString());
                    response.StatusCode = 428;
                    await response.WriteAsync(JsonConvert.SerializeObject(new {
                        Success = false,
                        Error = error.Message
                    }, _serializerSettings));
                } catch (UnauthorizedException error) {
                    _logger.LogWarning(error.ToString());
                    response.StatusCode = 401;
                    await response.WriteAsync(JsonConvert.SerializeObject(new {
                        Success = false,
                        Error = error.Message
                    }, _serializerSettings));
                } catch (InvalidatedException error) {
                    _logger.LogWarning(error.ToString());
                    response.StatusCode = 400;
                    await response.WriteAsync(JsonConvert.SerializeObject(new {
                        Success = false,
                        Error = error.Message
                    }, _serializerSettings));
                } catch (Exception error) {
                    _logger.LogCritical(error.ToString());
                    response.StatusCode = 500;
                    await response.WriteAsync(JsonConvert.SerializeObject(new {
                        Success = false,
                        Error = error.Message
                    }, _serializerSettings));
                }
            }
        }
    }
}
