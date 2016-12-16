using System;
using System.Collections.Generic;
using SprayChronicle.EventHandling;

namespace SprayChronicle.Testing
{
    public class TestStream : IStream
    {
        List<Action<object,DateTime>> _callbacks = new List<Action<object,DateTime>>();

        public void Publish(object message, DateTime epoch)
        {
            _callbacks.ForEach(callback => callback(message, epoch));
        }

        public void OnEvent(Action<object,DateTime> callback)
        {
            _callbacks.Add(callback);
        }
    }
}
