using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Shouldly;

namespace SprayChronicle.Testing
{
    public class HandlingValidator : IValidate
    {
        private readonly IContainer _container;
        
        private readonly Exception _error;
        
        private HandlingValidator(IContainer container)
        {
            _container = container;
        }
        
        private HandlingValidator(IContainer container, Exception error)
        {
            _container = container;
            _error = error;
        }

        public static async Task<HandlingValidator> Run(IContainer container, Func<Task> callback)
        {
            container.Resolve<TestStore>().Present();
            
            try {
                await callback();
            } catch (Exception error) {
                return new HandlingValidator(
                    container,
                    error
                );
            }
            
            return new HandlingValidator(
                container
            );
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
            _container
                .Resolve<TestStore>()
                .Future()
                .Count()
                .ShouldBe(count);
            
            return this;
        }

        public IValidate Expect(params object[] expectation)
        {
            _container
                .Resolve<TestStore>()
                .Future()
                .Select(dm => dm.Message)
                .ToArray()
                .ShouldBeDeepEqualTo(expectation);
            
            return this;
        }

		public IValidate Expect(params Type[] expectation)
		{
		    _container
		        .Resolve<TestStore>()
		        .Future()
		        .Select(dm => dm.Message.GetType().FullName)
		        .ToArray()
		        .ShouldBeDeepEqualTo(expectation);
            
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