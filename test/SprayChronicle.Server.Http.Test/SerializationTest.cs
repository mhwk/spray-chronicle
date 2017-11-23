using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace SprayChronicle.Server.Http.Test
{
    public class SerializationTest
    {
        [Fact]
        public void ItSerializesCorrectly()
        {
            JsonConvert.SerializeObject(new SerializeMe(null), new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter>() { new ISO8601DateConverter() }
            }).ShouldBeEquivalentTo("{\"optionalDateTime\":null}");
        }

        [Fact]
        public void ItDeserializesCorrectly()
        {
            JsonConvert.DeserializeObject<SerializeMe>("{\"optionalDateTime\":null}", new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter>() { new ISO8601DateConverter() }
            }).ShouldBeEquivalentTo(new SerializeMe(null));
        }

        public class SerializeMe
        {
            public DateTime? OptionalDateTime;

            public SerializeMe(DateTime? optionalDateTime)
            {
                OptionalDateTime = optionalDateTime;
            }
        }
    }
}
