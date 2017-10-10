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
        readonly ILogger<HttpQueryProcessor> _logger;

        readonly IAuthorizer _authorizer;

        readonly IValidator _validator;

        readonly IProcessQueries _dispatcher;

        readonly Type _type;

        readonly string _contentType;

        readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>() { new ISO8601DateConverter() }
        };
        
        static readonly RequestToMessageConverter converter = new RequestToMessageConverter();

        public HttpQueryProcessor(
            ILogger<HttpQueryProcessor> logger,
            IAuthorizer authorizer,
            IValidator validator,
            IProcessQueries dispatcher,
            Type type,
            string contentType)
        {
            _logger = logger;
            _authorizer = authorizer;
            _validator = validator;
            _dispatcher = dispatcher;
            _type = type;
            _contentType = contentType;
        }

        public async Task Process(HttpRequest request, HttpResponse response, RouteData routeData)
        {
            try {
                var payload = await converter.Convert(request, routeData, _type);

                // _authorizer.Authorize(payload, context);
                _validator.Validate(payload);

                _logger.LogDebug("Processing {0} {1}", _type, JsonConvert.SerializeObject(payload));
                var result = _dispatcher.Process(payload);
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
            } catch (InvalidatedException error) {
                _logger.LogInformation(error.ToString());
                response.ContentType = "application/json";
                response.StatusCode = 400;
                await response.WriteAsync(JsonConvert.SerializeObject(new {
                    Error = error.Message
                }, _serializerSettings));
            } catch (UnauthorizedException error) {
                _logger.LogInformation(error.ToString());
                response.ContentType = "application/json";
                response.StatusCode = 401;
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