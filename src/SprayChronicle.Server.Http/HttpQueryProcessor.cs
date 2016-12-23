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

        readonly IAuthorizer _authorizer;

        readonly IValidator _validator;

        readonly IProcessQueries _dispatcher;

        readonly Type _type;

        static readonly RequestToMessageConverter converter = new RequestToMessageConverter();

        public HttpQueryProcessor(
            ILogger<HttpQueryProcessor> logger,
            IAuthorizer authorizer,
            IValidator validator,
            IProcessQueries dispatcher,
            Type type)
        {
            _logger = logger;
            _authorizer = authorizer;
            _validator = validator;
            _dispatcher = dispatcher;
            _type = type;
        }

        public async Task Process(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try {
                var payload = await converter.Convert(context.Request, _type);

                _authorizer.Authorize(payload, context);
                _validator.Validate(payload);

                _logger.LogDebug("Processing {0} {1}", _type, JsonConvert.SerializeObject(payload));
                var result = _dispatcher.Process(payload);
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(result));
            } catch (UnhandledQueryException error) {
                _logger.LogInformation(error.ToString());
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                    Error = error.InnerException.Message
                }));
            } catch (InvalidatedException error) {
                _logger.LogInformation(error.ToString());
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                    Error = error.Message
                }));
            } catch (UnauthorizedException error) {
                _logger.LogInformation(error.ToString());
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