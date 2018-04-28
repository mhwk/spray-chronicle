using System;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventSourcing
{
    public interface IEventSourcingRepository<T> where T : IEventSourcable<T>
    {
        Task Save(T subject, IEnvelope envelope);

        Task Save<TChild>(T subject, IEnvelope envelope) where TChild : T;

        Task<T> Load(string identity, string idempotencyId);

        Task<TChild> Load<TChild>(string identity, string idempotencyId) where TChild : T;

        Task<TChild> LoadOrDefault<TChild>(string identity, string idempotencyId) where TChild : T;
    }
}
