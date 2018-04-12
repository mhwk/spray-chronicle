using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Documents;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public class RavenDbRepository<T> : IStatefulRepository<T> where T : class
    {
        private readonly IDocumentStore _store;

        public RavenDbRepository(IDocumentStore store)
        {
            _store = store;
        }

        public string Identity(T obj)
        {
            using (var session = _store.OpenSession()) {
                return session.Advanced.GetDocumentId(obj);
            }
        }

        public void Save(T obj)
        {
            using (var session = _store.OpenSession()) {
                session.Store(obj);
                session.SaveChanges();
            }
        }

        public void Save(T[] objs)
        {
            using (var session = _store.OpenSession()) {
                foreach (var obj in objs) {
                    session.Store(obj);
                }
                
                session.SaveChanges();
            }
        }

        public T Load(string identity)
        {
            using (var session = _store.OpenSession()) {
                return session.Load<T>(identity);
            }
        }

        public T Load(Func<IQueryable<T>, T> callback)
        {
            using (var session = _store.OpenSession()) {
                return callback(session.Query<T>());
            }
        }

        public IEnumerable<T> Load(Func<IQueryable<T>, IEnumerable<T>> callback)
        {
            using (var session = _store.OpenSession()) {
                return callback(session.Query<T>());
            }
        }

        public PagedResult<T> Load(Func<IQueryable<T>, IEnumerable<T>> callback, int page, int perPage)
        {
            throw new NotImplementedException();
        }

        public void Remove(string identity)
        {
            throw new NotImplementedException();
        }

        public void Remove(string[] identities)
        {
            throw new NotImplementedException();
        }

        public void Remove(T obj)
        {
            throw new NotImplementedException();
        }

        public void Remove(T[] objs)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Start(Func<T> callback)
        {
            throw new NotImplementedException();
        }

        public void With(string id, Func<T, T> callback)
        {
            throw new NotImplementedException();
        }
    }
}