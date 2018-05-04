using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace SprayChronicle.Persistence.Mongo
{
    public sealed class MongoExecutedSingle<TState> : MongoExecuted
        where TState : class
    {
        private readonly Func<IMongoQueryable<TState>,Task<TState>> _query;

        public MongoExecutedSingle(Func<IMongoQueryable<TState>,Task<TState>> query)
        {
            _query = query;
        }
        
        internal override async Task<object> Do(IMongoDatabase database)
        {
            return await _query(database.GetCollection<TState>(typeof(TState).Name).AsQueryable());
        }
    }
}
