using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.Testing
{
    public sealed class TestStream : ITestableStream
    {
        private readonly List<Action<IMessage,DateTime>> _callbacks = new List<Action<IMessage,DateTime>>();
        
        private readonly EpochGenerator _epochs;

        public TestStream(EpochGenerator epochs)
        {
            _epochs = epochs;
        }

        public ITestableStream Epochs(params DateTime[] epochs)
        {
            foreach (var dateTime in epochs) {
                _epochs.Add(dateTime);
            }
            
            return this;
        }

        public ITestableStream Epochs(params string[] epochs)
        {
            foreach (var dateTime in epochs) {
                _epochs.Add(dateTime);
            }

            return this;
        }

        public Task Publish(params object[] messages)
        {
            var i = 0;

            foreach (var message in messages) {
                var index = i;
                _callbacks.ForEach(callback => callback(message.ToMessage(), _epochs[index]));

                i++;
            }
            
            return Task.CompletedTask;
        }

        public void Subscribe(Action<IMessage,DateTime> callback)
        {
            _callbacks.Add(callback);
        }
    }
}
