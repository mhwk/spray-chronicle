using System;
using System.Collections.Generic;
using SprayChronicle.EventHandling;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.Testing
{
    public sealed class TestStream : IStream
    {
        private readonly List<Action<IMessage,DateTime>> _callbacks = new List<Action<IMessage,DateTime>>();

        public void Publish(IMessage message, DateTime epoch)
        {
            _callbacks.ForEach(callback => callback(message, epoch));
        }

        public void Subscribe(Action<IMessage,DateTime> callback)
        {
            _callbacks.Add(callback);
        }
    }
}
