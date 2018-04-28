using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using SprayChronicle.EventSourcing;
using Shouldly;

namespace SprayChronicle.Testing
{
    public class EventSourcedValidator : IValidate
    {
        private readonly IContainer _container;

        private readonly Tuple<long,object,DateTime>[] _messages;
        
        private readonly Exception _error;
        
        private EventSourcedValidator(IContainer container, Tuple<long,object,DateTime>[] messages)
        {
            _container = container;
            _messages = messages;
        }
        
        private EventSourcedValidator(IContainer container, Exception error)
        {
            _container = container;
            _messages = new Tuple<long,object,DateTime>[] {};
            _error = error;
        }

        public static async Task<EventSourcedValidator> Run<TSourced>(IContainer container, Func<Task<TSourced>> callback)
            where TSourced : EventSourced<TSourced>
        {
            try {
                return new EventSourcedValidator(
                    container,
                    (await callback())?.Diff().ToArray()
                );
            } catch (Exception error) {
                return new EventSourcedValidator(
                    container,
                    error
                );
            }
        }
        
        public DateTime Epoch(int index)
        {
            return _container.Resolve<EpochGenerator>()[index];
        }

		public IValidate Expect()
        {
            Expect(new object[] { });
            
            return this;
        }

		public IValidate Expect(int count)
        {
            _messages.Length.ShouldBe(count);
            
            return this;
        }

		public IValidate Expect(params object[] expectation)
		{
		    _messages
		        .Select(dm => dm.Item2)
		        .ToArray()
		        .ShouldBeDeepEqualTo(expectation);
            
            return this;
        }

		public IValidate Expect(params Type[] expectation)
		{
		    _messages
		        .Select(dm => dm.Item2.GetType().FullName)
		        .ToArray()
		        .ShouldBeDeepEqualTo(expectation
		            .Select(type => type.FullName)
		            .ToArray()
		        );
            
            return this;
        }

		public IValidate ExpectNoException()
        {
            _error.ShouldBeNull(_error?.ToString());
            
            return this;
        }

		public IValidate ExpectException(Type type)
        {
            if (null == type) {
                ExpectNoException();
            } else {
                _error.ShouldBeOfType(type, _error?.ToString());
            }
            return this;
        }

		public IValidate ExpectException(string message)
        {
            _error.Message.ShouldBe(message);
            return this;
        }
    }
}