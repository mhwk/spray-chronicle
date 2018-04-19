using Autofac;

namespace SprayChronicle.Server.Http
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterChronicleHttp(this ContainerBuilder builder)
        {
            builder.RegisterModule<ChronicleServerHttpModule>();
        }
    }
}