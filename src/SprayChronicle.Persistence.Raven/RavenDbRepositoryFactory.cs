using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public class RavenDbRepositoryFactory : IBuildStatefulRepositories
    {
        private readonly IDocumentStore _store;

        private readonly ILoggerFactory _loggerFactory;

        public RavenDbRepositoryFactory(
            IDocumentStore store,
            ILoggerFactory loggerFactory)
        {
            _store = store;
            _loggerFactory = loggerFactory;
        }

        public IStatefulRepository<T> Build<T>() where T : class
        {
            return new BufferedStateRepository<T>(
                _loggerFactory.CreateLogger<T>(),
                new RavenDbRepository<T>(_store)
            );
        }

        public IStatefulRepository<T> Build<T>(string reference) where T : class
        {
            return new BufferedStateRepository<T>(
                _loggerFactory.CreateLogger<T>(),
                new RavenDbRepository<T>(_store)
            );
        }
    }
}