using System;

namespace SprayChronicle.MessageHandling
{
    public sealed class InstanceMessage : IMessage
    {
        private readonly object _instance;
        
        public string Type => _instance.GetType().Name;

        public InstanceMessage(object instance)
        {
            _instance = instance;
        }

        public object Instance()
        {
            return _instance;
        }

        public object Instance(Type type)
        {
            if (!_instance.GetType().IsAssignableFrom(type)) {
                throw new UnhandledMessageException(string.Format(
                    "Message {0} is not convertable to {1}",
                    _instance.GetType(),
                    type
                ));
            }
            return _instance;
        }
    }
}
