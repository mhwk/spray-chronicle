using Autofac;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Server;

namespace SprayChronicle.CommandHandling
{
    public static class CommandHandlingExtensions
    {
        public static ChronicleServer WithCommandHandling(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterModule<CommandHandlingModule>();
            return server;
        }

        public static ContainerBuilder RegisterCommandHandler<THandler,TState>(this ContainerBuilder builder, string streamName = null)
            where THandler : class, IHandle, IProcess
            where TState : EventSourced<TState>
        {
            builder.RegisterModule(new CommandHandlingModule.CommandPipeline<THandler,TState>(streamName));
            return builder;
        }
    }
}
