using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Autofac;

namespace SprayChronicle.Server
{
    public sealed class ChronicleServer
    {
        public void Run()
        {
            ChronicleLogging.Subscribe(this);
            
            var webHost = WebHost.CreateDefaultBuilder()
                .ConfigureServices(services => services.AddSingleton<IStartup>(new Startup(this)))
                .ConfigureServices(services => services.AddSingleton(this))
                .UseStartup<Startup>();

            OnWebHostBuild?.Invoke(webHost);

            webHost.Build().Run();
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
    }
}
