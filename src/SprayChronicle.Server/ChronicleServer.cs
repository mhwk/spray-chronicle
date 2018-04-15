using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Autofac;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.CommandLineUtils;

namespace SprayChronicle.Server
{
    public sealed class ChronicleServer
    {
        public static ImmutableDictionary<string, string> Variables { get; private set; } = ImmutableDictionary.Create<string,string>();
        
        public async Task<int> Run(params string[] args)
        {
            ChronicleLogging.Subscribe(this);
            
            var webHost = WebHost.CreateDefaultBuilder()
                .ConfigureServices(services => services.AddSingleton<IStartup>(new Startup(this)))
                .ConfigureServices(services => services.AddSingleton(this))
                .UseStartup<Startup>();

            OnWebHostBuild?.Invoke(webHost);

            var host = webHost.Build();
            var cli = host.Services.GetService<CommandLineApplication>();

            cli.OnExecute(() => {
                host.Run();
                return 0;
            });

            return await Task.Run(() => cli.Execute(args)).ConfigureAwait(false);
        }

        public delegate void StartupArgs(IServiceProvider services);
        public delegate void ShutdownArgs(IServiceProvider services);
        public delegate void ConfigureAutofacArgs(ContainerBuilder builder);
        public delegate void ConfigureServicesArgs(IServiceCollection services);
        public delegate void BuildApplicationArgs(IApplicationBuilder services);
        public delegate void BuildWebHostArgs(IWebHostBuilder webHost);
        
        public StartupArgs OnStartup;
        public ShutdownArgs OnShutdown;
        public ConfigureAutofacArgs OnAutofacConfigure;
        public ConfigureServicesArgs OnServiceConfigure;
        public BuildApplicationArgs OnApplicationBuild;
        public BuildWebHostArgs OnWebHostBuild;

        public static string Env(string variableName)
        {
            if (!Variables.ContainsKey(variableName)) {
                var value = Environment.GetEnvironmentVariable(variableName);
            
                if (null == value) {
                    throw new MissingConfigurationException($"Environment variable {variableName} not set");
                }

                Variables = Variables.Add(variableName, value);
            }

            return Variables[variableName];
        }

        public static string Env(string variableName, string defaultValue)
        {
            if (!Variables.ContainsKey(variableName)) {
                var value = Environment.GetEnvironmentVariable(variableName);

                Variables = Variables.Add(variableName, value ?? defaultValue);
            }

            return Variables[variableName];
        }
    }
}
