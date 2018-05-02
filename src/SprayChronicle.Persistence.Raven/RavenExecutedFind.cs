using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace SprayChronicle.Persistence.Raven
{
    public class RavenExecutedFind<TState> : RavenExecuted
        where TState : class
    {
        private readonly string _identity;

        public RavenExecutedFind(string identity)
        {
            _identity = identity;
        }
        
        internal override async Task<object> Do(IAsyncDocumentSession session)
        {
            return await session.LoadAsync<TState>($"{typeof(TState).Name}/{_identity}");
        }
    }
}
