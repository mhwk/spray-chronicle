using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Shouldly;

namespace SprayChronicle.Testing
{
    public class QueryValidator : IValidate
    {
        private readonly IContainer _container;

        private readonly object[] _result;
        
        private readonly Exception _error;

        private QueryValidator(IContainer container, Exception error)
        {
            _container = container;
            _result = new object[] { };
            _error = error;
        }

        private QueryValidator(IContainer container, object result)
        {
            _container = container;

            if (result is IEnumerable<object> iterable) {
                _result = iterable.ToArray();
            } else {
                _result = new [] { result };
            }
        }

        public static async Task<QueryValidator> Run<T>(IContainer container, Func<Task<T>> callback)
        {
            try {
                return new QueryValidator(
                    container,
                    await callback()
                );
            } catch (Exception error) {
                return new QueryValidator(
                    container,
                    error
                );
            }
        }

        public IValidate Expect()
        {
            _result.ShouldBeNull();
            
            return this;
        }

		public IValidate Expect(int count)
        {
            _result.Length.ShouldBe(count);
            
            return this;
        }

		public IValidate Expect(params object[] expectation)
		{
		    if (null == expectation) {
		        return Expect();
		    }

		    _result.ShouldBeDeepEqualTo(expectation);
		    
            return this;
        }

		public IValidate Expect(params Type[] expectation)
		{
		    _result
		        .Select(p => p.GetType().AssemblyQualifiedName)
		        .ToArray()
		        .ShouldBeDeepEqualTo(
		            expectation
		                .Select(t => t.AssemblyQualifiedName)
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
                return ExpectNoException();
            }
            
            _error.ShouldBeOfType(type, _error?.ToString());
            
            return this;
        }

		public IValidate ExpectException(string message)
        {
            _error.Message.ShouldBe(message);
            
            return this;
        }
    }
}
