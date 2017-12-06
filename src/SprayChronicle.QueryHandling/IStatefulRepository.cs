using System;
using System.Linq;
using System.Collections.Generic;

namespace SprayChronicle.QueryHandling
{
    public interface IStatefulRepository<T> where T : class
    {
        string Identity(T obj);

        void Save(T obj);

        void Save(T[] objs);

        T Load(string identity);

        T Load(Func<IQueryable<T>,T> callback);

        IEnumerable<T> Load(Func<IQueryable<T>,IEnumerable<T>> callback);

        PagedResult<T> Load(Func<IQueryable<T>,IEnumerable<T>> callback, int page, int perPage);

        void Remove(string identity);

        void Remove(string[] identities);

        void Remove(T obj);

        void Remove(T[] objs);

        void Clear();

        void Start(Func<T> callback);

        void With(string id, Func<T, T> callback);
    }
}
