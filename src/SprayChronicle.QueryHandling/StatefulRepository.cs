using System;
using System.Collections.Generic;
using System.Linq;

namespace SprayChronicle.QueryHandling
{
    public abstract class StatefulRepository<T> : IStatefulRepository<T> where T : class
    {
        public void Start(Func<T> callback)
        {
            Save(callback());
        }

        public void With(string id, Func<T,T> callback)
        {
            var projection = Load(id);
            if (null == projection) {
                throw new ProjectionException(string.Format(
                    "Projection {0} with id {1} does not exist", typeof(T), id
                ));
            }
            Save(callback(projection));
        }

        public abstract string Identity(T obj);
        public abstract void Save(T obj);
        public abstract void Save(T[] objs);
        public abstract T Load(string identity);
        public abstract T Load(Func<IQueryable<T>, T> callback);
        public abstract IEnumerable<T> Load(Func<IQueryable<T>, IEnumerable<T>> callback);
        public abstract PagedResult<T> Load(Func<IQueryable<T>, IEnumerable<T>> callback, int page, int perPage);
        public abstract void Remove(string identity);
        public abstract void Remove(string[] identities);
        public abstract void Remove(T obj);
        public abstract void Remove(T[] objs);
        public abstract void Clear();
    }
}