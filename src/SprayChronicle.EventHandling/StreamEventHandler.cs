using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.EventHandling
{
    public sealed class StreamEventHandler<T> : IHandleStream where T: IHandleEvent
    {
        readonly ILogger<IStream> _logger;

        readonly IStream _stream;

        readonly IHandleEvent _eventHandler;

        readonly Dictionary<Type,MethodInfo> _handlers = new Dictionary<Type,MethodInfo>();

        public StreamEventHandler(
            ILogger<IStream> logger,
            IStream stream,
            IHandleEvent eventHandler)
        {
            _logger = logger;
            _stream = stream;
            _eventHandler = eventHandler;

            foreach (var method in eventHandler.GetType().GetTypeInfo().GetMethods().Where(m => m.GetParameters().Length > 0)) {
                _handlers.Add(method.GetParameters()[0].ParameterType, method);
            }
        }

        public async Task ListenAsync()
        {
            await Task.Run(() => Listen());
        }

        void Listen()
        {
            _stream.OnEvent((@event, occurrence) => {
                _logger.LogInformation(
                    "[{0}::{1}] skipping",
                    _eventHandler.GetType().Name,
                    @event.GetType().Name
                );
                if ( ! _handlers.ContainsKey(@event.GetType())) {
                    return;
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                Invoke(_handlers[@event.GetType()], @event, occurrence);

                stopwatch.Stop();
                _logger.LogInformation(
                    "[{0}::{1}] {2}ms",
                    _eventHandler.GetType().Name,
                    @event.GetType().Name,
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

            method.Invoke(_eventHandler, args.ToArray());
        }
    }
}
