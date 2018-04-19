using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.Server.Http
{
    public class HttpCommandDispatcher
    {
        private readonly ILogger<HttpCommandDispatcher> _logger;

        private readonly IValidator _validator;

        private readonly CommandRouter _dispatcher;

        private readonly Type _type;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>() { new ISO8601DateConverter() }
        };

        private static readonly RequestToMessageConverter converter = new RequestToMessageConverter();

        public HttpCommandDispatcher(
            ILogger<HttpCommandDispatcher> logger,
            IValidator validator,
            CommandRouter dispatcher,
            Type type)
        {
            _logger = logger;
            _validator = validator;
            _dispatcher = dispatcher;
            _type = type;
        }

        public async Task Dispatch(HttpRequest request, HttpResponse response, RouteData routeData)
        {
            response.ContentType = "application/json";

            try {
                var payload = await converter.Convert(request, routeData, _type);

                _validator.Validate(payload);

                _logger.LogDebug("Dispatching {0} {1}", _type, JsonConvert.SerializeObject(payload));
                await _dispatcher.Route(payload);
                response.StatusCode = 200;
                await response.WriteAsync(JsonConvert.SerializeObject(new {
                    Success = true
                }, _serializerSettings));
            } catch (UnhandledCommandException error) {
                _logger.LogWarning(error);
                response.StatusCode = 404;
                await response.WriteAsync(JsonConvert.SerializeObject(new {
                    Success = false,
                    Error = error.Message
                }, _serializerSettings));
            } catch (ConcurrencyException error) {
                _logger.LogWarning(error);
                response.StatusCode = 409;
                await response.WriteAsync(JsonConvert.SerializeObject(new {
                    Success = false,
                    Error = error.Message
                }, _serializerSettings));
            } catch (InvalidStateException error) {
                _logger.LogWarning(error);
                response.StatusCode = 428;
                await response.WriteAsync(JsonConvert.SerializeObject(new {
                    Success = false,
                    Error = error.Message
                }, _serializerSettings));
            } catch (InvalidRequestException error) {
                _logger.LogWarning(error);
                response.StatusCode = 400;
                await response.WriteAsync(JsonConvert.SerializeObject(new {
                    Success = false,
                    Error = error.Message
                }, _serializerSettings));
            } catch (Exception error) {
                _logger.LogCritical(error);
                response.StatusCode = 500;
                await response.WriteAsync(JsonConvert.SerializeObject(new {
                    Success = false,
                    Error = error.Message
                }, _serializerSettings));
            }
        }
    }
}
