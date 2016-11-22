namespace SprayChronicle.EventSourcing
{
    public interface IEventRouter<T> where T : IEventSourcable<T>
    {
        IEventSourcable<T> Route(IEventSourcable<T> sourcable, DomainMessage domainMessage);
    }
}
