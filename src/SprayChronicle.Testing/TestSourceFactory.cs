using System;
using System.Collections.Generic;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Server;

namespace SprayChronicle.Testing
{
    public class TestSourceFactory : IEventSourceFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        
        private readonly Dictionary<Type, IEventSource> _sources = new Dictionary<Type, IEventSource>();

        public TestSourceFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public IEventSource<TTarget> Build<TTarget, TOptions>(TOptions options) where TTarget : class
        {
            if (! _sources.ContainsKey(typeof(TTarget))) {
                _sources.Add(typeof(TTarget), new TestSource<TTarget>(
                    _loggerFactory.Create<TTarget>()
                ));
            }
            return _sources[typeof(TTarget)] as IEventSource<TTarget>;
        }
    }
}