using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Server.Http
{
    public class HttpQueryProcessor
    {
        private readonly ILogger<HttpQueryProcessor> _logger;

        private readonly IValidator _validator;

        private readonly IProcessQueries _dispatcher;

        private readonly Type _type;

        private readonly string _contentType;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>() { new ISO8601DateConverter() }
        };

        private static readonly RequestToMessageConverter Converter = new RequestToMessageConverter();

        public HttpQueryProcessor(
            ILogger<HttpQueryProcessor> logger,
            IValidator validator,
            IProcessQueries dispatcher,
            Type type,
            string contentType)
        {
            _logger = logger;
            _validator = validator;
            _dispatcher = dispatcher;
            _type = type;
            _contentType = contentType;
        }

        public async Task Process(HttpRequest request, HttpResponse response, RouteData routeData)
        {
            try {
                var payload = await Converter.Convert(request, routeData, _type);

                _validator.Validate(payload);

                _logger.LogDebug("Processing {0} {1}", _type, JsonConvert.SerializeObject(payload));
                var result = await _dispatcher.Process(payload);
                response.ContentType = _contentType;
                response.StatusCode = 200;
                if (_contentType == "application/json") {
                    await response.WriteAsync(JsonConvert.SerializeObject(result, _serializerSettings));
                } else if (result is string) {
                    await response.WriteAsync((string) result);
                } else {
                    throw new UnexpectedContentException(string.Format(
                        "Could not send output of type {0} as content type {1}",
                        result.GetType(),
                        _contentType
                    ));
                }
            } catch (UnhandledQueryException error) {
                _logger.LogInformation(error.ToString());
                response.ContentType = "application/json";
                response.StatusCode = 400;
                await response.WriteAsync(JsonConvert.SerializeObject(new {
                    Error = error.InnerException.Message
                }, _serializerSettings));
            } catch (InvalidRequestException error) {
                _logger.LogInformation(error.ToString());
                response.ContentType = "application/json";
                response.StatusCode = 400;
                await response.WriteAsync(JsonConvert.SerializeObject(new {
                    Error = error.Message
                }, _serializerSettings));
            } catch (Exception error) {
                _logger.LogCritical(error.ToString());
                response.ContentType = "application/json";
                response.StatusCode = 500;
                await response.WriteAsync(JsonConvert.SerializeObject(new {
                    Error = error.Message
                }, _serializerSettings));
            }
        }

    }
}