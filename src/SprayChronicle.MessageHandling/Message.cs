using System;

namespace SprayChronicle.MessageHandling
{
    public sealed class Message : IMessage
    {
        public string Name { get; }

        private readonly object _payload;
        
        public Message(object payload)
        {
            Name = payload.GetType().Name;
            _payload = payload;
        }

        public object Payload()
        {
            return _payload;
        }

        public object Payload(Type type)
        {
            if (!_payload.GetType().IsAssignableFrom(type)) {
                throw new IncompatibleMessageException(string.Format(
                    "Message {0} is not convertable to {1}",
                    _payload.GetType(),
                    type
                ));
            }
            return _payload;
        }
    }
}