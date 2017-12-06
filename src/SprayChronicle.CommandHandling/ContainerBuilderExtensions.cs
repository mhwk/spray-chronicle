using Autofac;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.CommandHandling
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterCommandHandling(this ContainerBuilder builder)
        {
            builder.RegisterModule<CommandHandlingModule>();
            return builder;
        }

        public static ContainerBuilder RegisterCommandHandler<TSourced,THandler>(this ContainerBuilder builder)
            where TSourced : EventSourced<TSourced>
            where THandler : IHandleCommands, IHandleEvents
        {
            builder.RegisterModule(new CommandHandlingModule.CommandHandler<TSourced,THandler>());
            return builder;
        }

        public static ContainerBuilder RegisterCommandHandler<TSourced,THandler>(this ContainerBuilder builder, string stream)
            where TSourced : EventSourced<TSourced>
            where THandler : IHandleCommands, IHandleEvents
        {
            builder.RegisterModule(new CommandHandlingModule.CommandHandler<TSourced,THandler>(stream));
            return builder;
        }
    }
}
