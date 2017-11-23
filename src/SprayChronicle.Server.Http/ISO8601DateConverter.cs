using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SprayChronicle.Server.Http
{
    public class ISO8601DateConverter : DateTimeConverterBase
    {
        readonly string _format = "yyyy-MM-ddTHH:mm:sszzz";

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (null == reader.Value) {
                return null;
            }
            if ( ! DateTime.TryParseExact(reader.Value.ToString(), _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) {
                return null;
            }
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (null != value) {
                writer.WriteValue(((DateTime)value).ToString(_format));
            }
        }
    }
}
