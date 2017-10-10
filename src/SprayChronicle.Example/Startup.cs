using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SprayChronicle.Server;
using SprayChronicle.Server.Http;
using Autofac;

namespace SprayChronicle.Example
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseChronicleRouting();
            app.UseCors(policy => policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()
                .AllowCredentials()
            );
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.UseChronicle();
            builder.UseChronicleHttp();

            #if !DEBUG
            // builder.UseMongoPersistence();
            // builder.UseOuroPersistence();
            #endif

            builder.UseModule<ExampleModule>();
        }
    }
}
