using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SprayChronicle.Server.Http
{
    public class ISO8601DateConverter : DateTimeConverterBase
    {
        readonly string _format = "yyyy-MM-ddTHH:mm:sszzz";

        //
        // Summary:
        //     /// Reads the JSON representation of the object. ///
        //
        // Parameters:
        //   reader:
        //     The Newtonsoft.Json.JsonReader to read from.
        //
        //   objectType:
        //     Type of the object.
        //
        //   existingValue:
        //     The existing value of object being read.
        //
        //   serializer:
        //     The calling serializer.
        //
        // Returns:
        //     The object value.
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return DateTime.ParseExact(reader.Value.ToString(), _format, CultureInfo.InvariantCulture);
        }

        //
        // Summary:
        //     /// Writes the JSON representation of the object. ///
        //
        // Parameters:
        //   writer:
        //     The Newtonsoft.Json.JsonWriter to write to.
        //
        //   value:
        //     The value.
        //
        //   serializer:
        //     The calling serializer.
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((DateTime)value).ToString(_format));
        }
    }
}