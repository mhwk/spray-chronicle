using System;
using System.Threading.Tasks;

namespace SprayChronicle.EventSourcing
{
    public interface IEventSourcingRepository<T> where T : IEventSourcable<T>
    {
        Task Save(T subject);

        Task Save<TChild>(T subject) where TChild : T;

        Task<T> Load(string identity);

        Task<TChild> Load<TChild>(string identity) where TChild : T;

        Task<TChild> LoadOrDefault<TChild>(string identity) where TChild : T;
    }
}
