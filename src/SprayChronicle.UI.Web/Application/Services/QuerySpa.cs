using System.Threading.Tasks;
using SprayChronicle.QueryHandling;
using SprayChronicle.UI.Web.Infrastructure;

namespace SprayChronicle.UI.Web.Application.Services
{
    public sealed class QuerySpa : IExecute<Index>
    {
        public Task<Executor> Execute(Index query)
        {
            return Task.FromResult((Executor) new StaticFileExecutor("index.html"));
        }
    }
}
