using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public class EventSourcedFixture<THandler,TSource> : IPopulate, IExecute where THandler : IHandleCommand where TSource: EventSourced<TSource>
    {
        readonly IContainer _container;

        TestRepository<TSource> _repository;

        int _sequence = -1;

        public EventSourcedFixture(): this(new ContainerBuilder().Build())
        {}

        public EventSourcedFixture(IContainer container)
        {
            _container = container;
        }

		public IExecute Given(params object[] messages)
        {
            Repository().History(messages.Select(payload => new DomainMessage(
                ++_sequence,
                new DateTime(),
                payload
            )).ToList());
            return this;
        }

		public IValidate When(object message)
        {
            Exception e = null;
            try {
                BuildHandler().Handle(message);
            } catch (Exception error) {
                e = error;
            }
            return new TestValidator(Repository().Future(), e);
        }

        protected virtual TestRepository<TSource> Repository()
        {
            if (null == _repository) {
                _repository = new TestRepository<TSource>();
            }
            return _repository;
        }
        
        protected virtual IHandleCommand BuildHandler()
        {
            return (THandler) Activator.CreateInstance(typeof(THandler), BuildArguments());
        }

        object[] BuildArguments()
        {
            List<object> args = new List<object>();
            args.Add(Repository());

            var constructor = typeof(THandler).GetTypeInfo().GetConstructors()
                .Where(c => c.GetParameters().Length > 1)
                .FirstOrDefault();
            
            if (null == constructor) {
                return args.ToArray();
            }

            var types = constructor.GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();
            
            for (var i = 1; i < types.Length; i++) {
                args.Add(_container.Resolve(types[i]));
            }

            return args.ToArray();
        }
    }
}
