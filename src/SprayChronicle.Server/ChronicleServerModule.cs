using System;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.Server
{
    public class ChronicleServerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<IApplicationLifetime>(c => new ApplicationLifetime(c.Resolve<ILoggerFactory>().CreateLogger<ApplicationLifetime>()))
                .OnActivating(e => {
                    var server = e.Context.Resolve<ChronicleServer>();
                    var provider = e.Context.Resolve<IServiceProvider>();
                    var lifetime = e.Instance as ApplicationLifetime;

                    lifetime.ApplicationStarted.Register(() => { if (null != server.OnStartup) { server.OnStartup(provider); } });
                    lifetime.ApplicationStopping.Register(() => { if (null != server.OnShutdown) { server.OnShutdown(provider); } });
                })
                .SingleInstance();
        }
    }
}
