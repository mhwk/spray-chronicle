using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.Testing
{
    public sealed class TestStream : ITestableStream
    {
        private readonly List<DateTime> _epochs = new List<DateTime>();
        private readonly List<Action<IMessage,DateTime>> _callbacks = new List<Action<IMessage,DateTime>>();

        public ITestableStream Epochs(params DateTime[] epochs)
        {
            _epochs.AddRange(epochs);
            return this;
        }

        public ITestableStream Epochs(params string[] epochs)
        {
            return Epochs(epochs.Select(epoch => {
                if (!DateTime.TryParseExact(epoch, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) {
                    throw new FormatException(string.Format(
                        "Could not convert {0} into a valid date",
                        epoch
                    ));
                }
                return result;
            }).ToArray());
        }

        public Task Publish(params object[] messages)
        {
            var i = 0;

            foreach (var message in messages) {
                if (i >= _epochs.Count) {
                    _epochs.Add(DateTime.Now);
                }
                
                _callbacks.ForEach(callback => callback(message.ToMessage(), _epochs[i]));

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
