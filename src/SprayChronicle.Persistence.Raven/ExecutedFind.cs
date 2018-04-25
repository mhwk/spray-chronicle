using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace SprayChronicle.Persistence.Raven
{
    public class ExecutedFind<TState> : Executed
        where TState : class
    {
        private readonly string _identity;

        public ExecutedFind(string identity)
        {
            _identity = identity;
        }
        
        internal override async Task<object> Do(IAsyncDocumentSession session)
        {
            return await session.LoadAsync<TState>(_identity);
        }
    }
}
