using Autofac;

namespace SprayChronicle.EventHandling
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterEventHandling(this ContainerBuilder builder)
        {
            builder.RegisterModule<AsyncEventHandlingModule>();
            return builder;
        }
    }
}
