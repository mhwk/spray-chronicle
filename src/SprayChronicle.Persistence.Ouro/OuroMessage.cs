using System;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroMessage : IDomainMessage
    {
        private readonly ResolvedEvent _resolvedEvent;
        
        public string Name => _resolvedEvent.Event.EventType;
        
        public long Sequence => _resolvedEvent.Event.EventNumber;
        
        public DateTime Epoch => _resolvedEvent.Event.Created;

        public OuroMessage(ResolvedEvent resolvedEvent)
        {
            _resolvedEvent = resolvedEvent;
        }

        public object Payload()
        {
            return JsonConvert.DeserializeObject(
                Encoding.UTF8.GetString(_resolvedEvent.Event.Data)
            );
        }

        public object Payload(Type type)
        {
            return JsonConvert.DeserializeObject(
                Encoding.UTF8.GetString(_resolvedEvent.Event.Data),
                type
            );
        }
    }
}