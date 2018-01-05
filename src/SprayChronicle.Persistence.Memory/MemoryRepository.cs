using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Immutable;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryRepository<T> : StatefulRepository<T> where T : class
    {
        private readonly FieldInfo _identifier;

        private readonly Dictionary<string,T> _data = new Dictionary<string,T>();

        public MemoryRepository()
        {
            _identifier = typeof(T).GetTypeInfo()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Public)
                .FirstOrDefault(f => null != f.GetCustomAttribute<IdentifierAttribute>());
            
            if (null == _identifier) {
                throw new Exception(string.Format(
                    "No identifier attribute set on projection {0}",
                    typeof(T)
                ));
            }
        }

        public override string Identity(T obj)
        {
            return (string) _identifier.GetValue(obj);
        }

        public override T Load(string identity)
        {
            if ( ! _data.ContainsKey(identity)) {
                return default(T);
            }
            return _data[identity];
        }

        public override T Load(Func<IQueryable<T>,T> callback)
        {
            return callback(_data.Values.AsQueryable());
        }

        public override IEnumerable<T> Load(Func<IQueryable<T>,IEnumerable<T>> callback)
        {
            return callback(_data.Values.AsQueryable()).ToImmutableArray();
        }

        public override PagedResult<T> Load(Func<IQueryable<T>,IEnumerable<T>> callback, int page, int perPage)
        {
            var results = callback(_data.Values.AsQueryable()); 
            return new PagedResult<T>(
                results.Skip((page - 1) * perPage).Take(perPage).ToImmutableArray().ToArray(),
                page,
                perPage,
                results.Count()
            );
        }

        public override void Save(T obj)
        {
            if (_data.ContainsKey(Identity(obj))) {
                _data.Remove(Identity(obj));
            }
            _data.Add(Identity(obj), obj);
        }

        public override void Save(T[] objs)
        {
            foreach (var obj in objs) {
                Save(obj);
            }
        }

        public override void Remove(string identity)
        {
            if (_data.ContainsKey(identity)) {
                _data.Remove(identity);
            }
        }

        public override void Remove(string[] identities)
        {
            foreach (var identity in identities) {
                Remove(identity);
            }
        }

        public override void Remove(T obj)
        {
            Remove(Identity(obj));
        }

        public override void Remove(T[] objs)
        {
            foreach (var obj in objs) {
                Remove(obj);
            }
        }

        public override void Clear()
        {
            _data.Clear();
        }
    }
}
