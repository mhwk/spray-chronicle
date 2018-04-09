using System;
using System.Linq;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.Server
{
    public class ChronicleServerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new MicrosoftLoggerFactory(c.Resolve<Microsoft.Extensions.Logging.ILoggerFactory>()))
                .As<ILoggerFactory>()
                .SingleInstance();
            
            builder
                .Register(c => new ApplicationLifetime(c.Resolve<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger<ApplicationLifetime>()))
                .As<IApplicationLifetime>()
                .OnActivating(e => {
                    var server = e.Context.Resolve<ChronicleServer>();
                    var provider = e.Context.Resolve<IServiceProvider>();
                    var lifetime = e.Instance as ApplicationLifetime;

                    lifetime.ApplicationStarted.Register(() => {
                        server.OnStartup?.Invoke(provider);
                    });
                    lifetime.ApplicationStopping.Register(() => {
                        server.OnShutdown?.Invoke(provider);
                    });
                })
                .SingleInstance();
            
            builder
                .Register(c => new CommandLineApplication())
                .OnActivated(e => {
                    e.Instance.Name = "SprayChronicle";
                    e.Instance.HelpOption("-h|--help");
                                
                    RegisterConsoleCommands(e.Context, e.Instance);
                })
                .SingleInstance();
        }

        private static void RegisterConsoleCommands(IComponentContext context, CommandLineApplication commandLine)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IConsoleCommand>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IConsoleCommand)
                .ToList()
                .ForEach(command => commandLine.Commands.Add(new CommandLineApplication {
                    Name = command.Name,
                    Description = command.Description,
                    Invoke = () => command.Execute().Result
                }));
        }
    }
}
