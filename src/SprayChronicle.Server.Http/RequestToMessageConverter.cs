using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace SprayChronicle.Server.Http
{
    public class RequestToMessageConverter
    {
        public async Task<object> Convert(HttpRequest request, Type target)
        {
            switch (request.Method) {
                case "POST":
                    using (var reader = new StreamReader(request.Body)) {
                        return JsonConvert.DeserializeObject(await reader.ReadToEndAsync(), target);
                    }
                case "GET":
                    return JsonConvert.DeserializeObject(QueryToJson(request.Query), target);
                default:
                    throw new Exception(string.Format("Unsupported method {0}", request.Method));
            }
        }

        string QueryToJson(IQueryCollection query)
        {
            var dict = new Dictionary<string,string>();
            foreach (var key in query.Keys) {
                StringValues @value = new StringValues();
                query.TryGetValue(key, out @value);
                dict.Add(key, @value.ToString());
            }
            return JsonConvert.SerializeObject(dict);
        }
    }
}