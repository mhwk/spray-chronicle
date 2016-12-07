using System;
using System.Linq;
using System.Collections.Generic;

namespace SprayChronicle.QueryHandling
{
    public interface IStatefulRepository<T>
    {
        string Identity(T obj);

        void Save(T obj);

        void Save(T[] objs);

        T Load(string identity);

        void Remove(string identity);

        void Remove(string[] identities);

        void Remove(T obj);

        void Remove(T[] objs);

        IEnumerable<T> FindBy(Func<IQueryable<T>,IEnumerable<T>> callback);

        void Clear();
    }
}
