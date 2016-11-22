using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SprayChronicle.EventHandling
{
    public abstract class StreamEventHandler : IHandleStream
    {
        readonly IStream _stream;

        readonly Dictionary<string,MethodInfo> _methods = new Dictionary<string,MethodInfo>();

        readonly Dictionary<string,Type> _types = new Dictionary<string,Type>();

        public StreamEventHandler(IStream stream)
        {
            _stream = stream;
            foreach (var method in GetType().GetTypeInfo().GetMethods().Where(m => m.GetParameters().Length > 0)) {
                _methods.Add(method.GetParameters()[0].ParameterType.Name, method);
                _types.Add(method.GetParameters()[0].ParameterType.Name, method.GetParameters()[0].ParameterType);
            }
        }

        public async void ListenAsync()
        {
            await Task.Run(() => Listen());
        }

        void Listen()
        {
            _stream.Read((name, payload, occurrence) => {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                if ( ! _methods.ContainsKey(name)) {
                    return;
                }

                Invoke(_methods[name], JsonConvert.DeserializeObject(payload, _types[name]), occurrence);

                stopwatch.Stop();
                Console.WriteLine(
                    "[{0}::{1}] {2}ms",
                    GetType().Name,
                    name,
                    stopwatch.ElapsedMilliseconds
                );
            });
        }

        void Invoke(MethodInfo method, object message, DateTime occurrence)
        {
            List<object> args = new List<object>();
            args.Add(message);

            if (method.GetParameters().Length >= 2 && method.GetParameters()[1].ParameterType == typeof(DateTime)) {
                args.Add(occurrence);
            }

            method.Invoke(this, args.ToArray());
        }
    }
}
