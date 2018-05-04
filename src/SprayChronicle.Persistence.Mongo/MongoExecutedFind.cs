using System.Threading.Tasks;
using MongoDB.Driver;

namespace SprayChronicle.Persistence.Mongo
{
    public class MongoExecutedFind<TState> : MongoExecuted
        where TState : class
    {
        private readonly string _identity;

        public MongoExecutedFind(string identity)
        {
            _identity = identity;
        }
        
        internal override async Task<object> Do(IMongoDatabase database)
        {
            return await database
                .GetCollection<TState>(typeof(TState).Name)
                .FindAsync();
        }
    }
}
