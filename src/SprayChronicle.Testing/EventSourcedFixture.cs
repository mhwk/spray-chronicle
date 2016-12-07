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
        readonly Action<ContainerBuilder> _configure;

        IContainer _container;

        int _sequence = -1;

        public EventSourcedFixture(): this(builder => {})
        {}

        public EventSourcedFixture(Action<ContainerBuilder> configure)
        {
            _configure = configure;
        } 

        public IContainer Container()
        {
            if (null == _container) {
                var builder = new ContainerBuilder();

                builder
                    .Register<TestStore>(c => new TestStore())
                    .AsSelf()
                    .As<IEventStore>()
                    .SingleInstance();

                builder
                    .Register<IEventSourcingRepository<TSource>>(c => new EventSourcedRepository<TSource>(c.Resolve<IEventStore>()))
                    .SingleInstance();
                
                _configure(builder);
                
                _container = builder.Build();
            }
            return _container;
        }

		public IExecute Given(params object[] messages)
        {
            Container().Resolve<TestStore>().Append<TSource>("", messages.Select(payload => new DomainMessage(
                ++_sequence,
                new DateTime(),
                payload
            )).ToArray());

            return this;
        }

		public IValidate When(object message)
        {
            Container().Resolve<TestStore>().Record();
            Exception e = null;
            try {
                BuildHandler().Handle(message);
            } catch (Exception error) {
                e = error;
            }
            return new TestValidator(Container().Resolve<TestStore>().Recorded(""), e);
        }
        
        protected virtual IHandleCommand BuildHandler()
        {
            return (THandler) Activator.CreateInstance(typeof(THandler), BuildArguments());
        }

        object[] BuildArguments()
        {
            var args = new List<object>();

            var constructor = typeof(THandler).GetTypeInfo().GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();
            
            if (null == constructor) {
                return args.ToArray();
            }

            var types = constructor.GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();
            
            for (var i = 0; i < types.Length; i++) {
                args.Add(Container().Resolve(types[i]));
            }

            return args.ToArray();
        }
    }
}
