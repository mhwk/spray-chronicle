using System.Threading.Tasks;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Mongo
{
    public abstract class MongoExecuted : Executed
    {
        internal abstract Task<object> Do(IMongoSession session);
    }
}
