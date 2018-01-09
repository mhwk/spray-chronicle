using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Shouldly;

namespace SprayChronicle.Testing
{
    public class CommandValidator : IValidate
    {
        private readonly IContainer _container;
        
        private readonly Exception _error;
        
        private CommandValidator(IContainer container)
        {
            _container = container;
        }
        
        private CommandValidator(IContainer container, Exception error)
        {
            _container = container;
            _error = error;
        }

        public static async Task<CommandValidator> Run(IContainer container, Func<Task> callback)
        {
            container.Resolve<TestStore>().Present();
            
            try {
                await callback();
            } catch (Exception error) {
                return new CommandValidator(
                    container,
                    error
                );
            }
            
            return new CommandValidator(
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
                .Select(dm => dm.Payload())
                .ToArray()
                .ShouldBeDeepEqualTo(expectation);
            
            return this;
        }

		public IValidate Expect(params Type[] expectation)
		{
		    _container
		        .Resolve<TestStore>()
		        .Future()
		        .Select(dm => dm.Payload().GetType().FullName)
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