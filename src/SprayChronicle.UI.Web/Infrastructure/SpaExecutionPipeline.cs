using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using SprayChronicle.QueryHandling;
using SprayChronicle.Server;

namespace SprayChronicle.UI.Web.Infrastructure
{
    public class SpaExecutionPipeline<TQueryExecutor> : ExecutionPipeline<TQueryExecutor>
        where TQueryExecutor : class, IExecute
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public SpaExecutionPipeline(
            ILogger<TQueryExecutor> logger,
            TQueryExecutor processor,
            IHostingEnvironment hostingEnvironment) : base(logger, processor)
        {
            _hostingEnvironment = hostingEnvironment;
            Console.WriteLine(hostingEnvironment.ContentRootPath);
            Console.WriteLine(hostingEnvironment.WebRootPath);
        }

        protected override async Task<object> Apply(Executor executor)
        {
            if (!(executor is StaticFileExecutor staticFile)) {
                throw new ArgumentException($"Executor can only be of type {typeof(StaticFileExecutor)}");
            }

            return await staticFile.Do(_hostingEnvironment.WebRootPath);
        }
    }
}
