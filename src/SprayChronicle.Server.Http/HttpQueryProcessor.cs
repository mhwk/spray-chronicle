using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        public async Task Process(HttpContext context)
        {
            try {
                var payload = await converter.Convert(context.Request, _type);

                _authorizer.Authorize(payload, context);
                _validator.Validate(payload);

                _logger.LogDebug("Processing {0} {1}", _type, JsonConvert.SerializeObject(payload));
                var result = _dispatcher.Process(payload);
                context.Response.ContentType = _contentType;
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializerSettings));
            } catch (UnhandledQueryException error) {
                _logger.LogInformation(error.ToString());
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                    Error = error.InnerException.Message
                }, _serializerSettings));
            } catch (InvalidatedException error) {
                _logger.LogInformation(error.ToString());
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                    Error = error.Message
                }, _serializerSettings));
            } catch (UnauthorizedException error) {
                _logger.LogInformation(error.ToString());
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                    Error = error.Message
                }, _serializerSettings));
            } catch (Exception error) {
                _logger.LogCritical(error.ToString());
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                    Error = error.Message
                }, _serializerSettings));
            }
        }

    }
}