using System;

namespace SprayChronicle.MessageHandling
{
    public sealed class Message : IMessage
    {
        public string Name { get; }
        
        public DateTime Epoch { get; }

        public object Payload { get; }
        
        public Message(object payload) : this(payload, DateTime.Now)
        {
        }
        
        public Message(object payload, DateTime epoch)
        {
            Name = payload.GetType().Name;
            Epoch = epoch;
            Payload = payload;
        }
    }
}