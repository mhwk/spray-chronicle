using System.Threading.Tasks;

namespace SprayChronicle.Persistence.Mongo
{
    public class MongoExecutedFind<TState> : MongoExecuted
        where TState : class
    {
        private readonly string _identity;

        public RavenExecutedFind(string identity)
        {
            _identity = identity;
        }
        
        internal override async Task<object> Do(IMongoSession session)
        {
            return await session.LoadAsync<TState>($"{typeof(TState).Name}/{_identity}");
        }
    }
}
