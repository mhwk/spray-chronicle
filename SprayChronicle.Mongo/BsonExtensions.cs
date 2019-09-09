using System;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization;

namespace SprayChronicle.Mongo
{
    public static class BsonExtensions
    {
        public static void RequirePropertySetters<T>(this BsonClassMap<T> classMap)
        {
            var properties = typeof(T).GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );

            foreach (var property in properties.Where(p => p.CanRead && !p.CanWrite)) {
                throw new ArgumentException(
                    $"Setters are required on property {property.Name} in class {typeof(T)} to allow snapshots"
                );
            }
            
            foreach (var property in properties.Where(p => p.CanRead && p.CanWrite)) {
                classMap.MapProperty(property.Name);
            }
        }
    }
}
