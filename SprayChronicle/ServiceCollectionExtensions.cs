using Microsoft.Extensions.DependencyInjection;

namespace SprayChronicle
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventSourcing(this IServiceCollection services)
        {
            services.AddSingleton(s => new CommandDispatcherFactory(
                s.GetRequiredService<IStoreEvents>(),
                s.GetRequiredService<IStoreSnapshots>()
            ));
            services.AddSingleton<CommandDispatcher.Factory>(s => s.GetRequiredService<CommandDispatcherFactory>().Build);

            return services;
        }
    }
}
