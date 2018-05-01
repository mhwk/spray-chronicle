using System.Collections.Generic;
using System.Linq;
using Autofac;
using SprayChronicle.EventSourcing;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public static class OuroExtensions
    {
        public static ChronicleServer WithOuroPersistence(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterOuroPersistence();
            return server;
        }
        
        public static ContainerBuilder RegisterOuroPersistence(this ContainerBuilder builder)
        {
            builder.RegisterModule<OuroModule>();
            return builder;
        }

        public static string BuildProjectionQuery(this StreamOptions streamOptions)
        {
            if (streamOptions.Categories.Length == 0) {
                return null;
            }
            
            var categoryList = string.Join(", ", streamOptions.Categories.Select(c => $"'{c}'").ToArray());
            var query = new List<string>
            {
                "fromCategories([" + categoryList + "])",
                "  .when({",
                "    $any: function(state, event) {",
                "      linkTo(\"" + streamOptions.TargetStream + "\", event);",
                "    }",
                "  });"
            };

            return string.Join("\n", query);
        }
    }
}
