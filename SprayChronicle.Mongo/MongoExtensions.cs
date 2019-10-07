using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SprayChronicle.Mongo
{
    public static class MongoExtensions
    {
        public static async Task<bool> CollectionExistsAsync(this IMongoDatabase database, string collectionName)
        {
            var collections = await database.ListCollectionsAsync(new ListCollectionsOptions
            {
                Filter = new BsonDocument("name", collectionName)
            });
            
            return await collections.AnyAsync();
        }
    }
}
