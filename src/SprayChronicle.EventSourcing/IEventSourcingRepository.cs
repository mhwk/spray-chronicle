using System;

namespace SprayChronicle.EventSourcing
{
    public interface IEventSourcingRepository<T> where T : IEventSourcable<T>
    {
        void Save(T subject);

        void Save<TChild>(T subject) where TChild : T;

        T Load(string identity);

        TChild Load<TChild>(string identity)
            where TChild : T;

        TChild LoadOrDefault<TChild>(string identity)
            where TChild : T;

        void Start<TResult>(Func<TResult> callback)
            where TResult : T;

        void Continue<TResult>(string identity, Func<TResult, TResult> callback)
            where TResult : T;

        void Continue<TInit, TResult>(string identity, Func<TInit, TResult> callback)
            where TInit : T
            where TResult : T;

        void Continue<TInit, TResult>(Func<TInit> load, Func<TInit, TResult> callback)
            where TInit : T
            where TResult : T;
    }
}
