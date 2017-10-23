using Autofac;

namespace SprayChronicle.CommandHandling
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterCommandHandling(this ContainerBuilder builder)
        {
            builder.RegisterModule<CommandHandlingModule>();
            return builder;
        }
    }
}
