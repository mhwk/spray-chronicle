using System;
using System.Collections.Generic;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public class TestSourceFactory : IEventSourceFactory
    {
        private readonly Dictionary<Type, IEventSource> _sources = new Dictionary<Type, IEventSource>();
        
        public IEventSource<TTarget> Build<TTarget, TOptions>(TOptions options) where TTarget : class
        {
            if (! _sources.ContainsKey(typeof(TTarget))) {
                _sources.Add(typeof(TTarget), new TestSource<TTarget>());
            }
            return _sources[typeof(TTarget)] as IEventSource<TTarget>;
        }
    }
}