using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace SprayChronicle.Server.Http
{
    public class RequestToMessageConverter
    {
        public async Task<object> Convert(HttpRequest request, RouteData routeData, Type target)
        {
            switch (request.Method) {
                case "POST":
                    return JsonConvert.DeserializeObject(await BuildPostData(request.Body, routeData), target);
                case "GET":
                    return JsonConvert.DeserializeObject(BuildGetData(request.Query, routeData), target);
                default:
                    throw new Exception(string.Format("Unsupported method {0}", request.Method));
            }
        }

        async Task<string> BuildPostData(Stream body, RouteData routeData)
        {
            using (var reader = new StreamReader(body)) {
                var decoded = JsonConvert.DeserializeObject<Dictionary<string,object>>(await reader.ReadToEndAsync());

                foreach (var item in routeData.Values) {
                    decoded.Add(item.Key, item.Value);
                }

                return JsonConvert.SerializeObject(decoded);
            }
        }

        string BuildGetData(IQueryCollection query, RouteData routeData)
        {
            var dict = new Dictionary<string,object>();
            foreach (var key in query.Keys) {
                StringValues @value = new StringValues();
                query.TryGetValue(key, out @value);
                dict.Add(key, @value.ToString());
            }
            foreach (var item in routeData.Values) {
                dict.Add(item.Key, item.Value);
            }
            return JsonConvert.SerializeObject(dict);
        }
    }
}