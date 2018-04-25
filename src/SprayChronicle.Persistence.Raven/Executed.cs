using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public abstract class Executed : Executor
    {
        internal abstract Task<object> Do(IAsyncDocumentSession session);
    }
}
