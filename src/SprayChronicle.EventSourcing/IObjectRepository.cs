namespace SprayChronicle.EventSourcing
{
    public interface IObjectRepository<T> where T : IEventSourcable<T>
    {
        void Save(T subject);

        void Save<TChild>(T subject) where TChild : T;

        T Load(string identity);

        TChild Load<TChild>(string identity) where TChild : T;
    }
}
