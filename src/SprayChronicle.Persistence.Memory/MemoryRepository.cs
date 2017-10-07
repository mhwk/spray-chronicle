using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Immutable;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryRepository<T> : IStatefulRepository<T>
    {
        readonly FieldInfo _identifier;

        readonly Dictionary<string,T> _data = new Dictionary<string,T>();

        public MemoryRepository()
        {
            _identifier = typeof(T).GetTypeInfo().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Public)
                .Where(f => null != f.GetCustomAttribute<IdentifierAttribute>())
                .FirstOrDefault();
            
            if (null == _identifier) {
                throw new Exception(string.Format(
                    "No identifier attribute set on projection {0}",
                    typeof(T)
                ));
            }
        }

        public string Identity(T obj)
        {
            return (string) _identifier.GetValue(obj);
        }

        public T Load(string identity)
        {
            if ( ! _data.ContainsKey(identity)) {
                return default(T);
            }
            return _data[identity];
        }

        public T Load(Func<IQueryable<T>,T> callback)
        {
            return callback(_data.Values.AsQueryable());
        }

        public IEnumerable<T> Load(Func<IQueryable<T>,IEnumerable<T>> callback)
        {
            return callback(_data.Values.AsQueryable()).ToImmutableArray();
        }

        public PagedResult<T> Load(Func<IQueryable<T>,IEnumerable<T>> callback, int page, int perPage)
        {
            var results = callback(_data.Values.AsQueryable()); 
            return new PagedResult<T>(
                results.Skip((page - 1) * perPage).Take(perPage).ToImmutableArray(),
                page,
                perPage,
                results.Count()
            );
        }

        public void Save(T obj)
        {
            if (_data.ContainsKey(Identity(obj))) {
                _data.Remove(Identity(obj));
            }
            _data.Add(Identity(obj), obj);
        }

        public void Save(T[] objs)
        {
            foreach (var obj in objs) {
                Save(obj);
            }
        }

        public void Remove(string identity)
        {
            if (_data.ContainsKey(identity)) {
                _data.Remove(identity);
            }
        }

        public void Remove(string[] identities)
        {
            foreach (var identity in identities) {
                Remove(identity);
            }
        }

        public void Remove(T obj)
        {
            Remove(Identity(obj));
        }

        public void Remove(T[] objs)
        {
            foreach (var obj in objs) {
                Remove(obj);
            }
        }

        public void Clear()
        {
            _data.Clear();
        }
    }
}
