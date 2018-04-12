using System;

namespace SprayChronicle.MessageHandling
{
    public sealed class Message : IMessage
    {
        public string Name { get; }
        
        public DateTime Epoch { get; }

        private readonly object _payload;
        
        public Message(object payload) : this(payload, DateTime.Now)
        {
        }
        
        public Message(object payload, DateTime epoch)
        {
            Name = payload.GetType().Name;
            Epoch = epoch;
            _payload = payload;
        }

        public object Payload()
        {
            return _payload;
        }
    }
}