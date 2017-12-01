using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;

namespace SprayChronicle.Testing
{
    public class QueryValidator : Validator
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

        public override IValidate Expect()
        {
            _result.Should().BeEmpty(Diff(_result, null));
            return this;
        }

		public override IValidate Expect(int count)
        {
            _result.Should().HaveCount(count);
            return this;
        }

		public override IValidate Expect(params object[] results)
		{
		    if (null == results) {
		        return Expect();
		    }
		    
		    Expect(results.Select(r => r.GetType()).ToArray());
            
            _result.ShouldAllBeEquivalentTo(results, Diff(_result, results));
		    
            return this;
        }

		public override IValidate Expect(params Type[] types)
		{
		    var expect = types.Select(t => t.AssemblyQualifiedName).ToArray();
		    var actual = _result.Select(p => p.GetType().AssemblyQualifiedName).ToArray();
		    
		    actual.ShouldAllBeEquivalentTo(expect, Diff(expect, actual));
		    
            return this;
        }

		public override IValidate ExpectNoException()
        {
            _error.Should().BeNull(_error?.ToString());
            return this;
        }

		public override IValidate ExpectException(Type type)
        {
            if (null == type) {
                return ExpectNoException();
            }
            
            _error.Should().BeOfType(type, _error?.ToString());
            return this;
        }

		public override IValidate ExpectException(string message)
        {
            _error.Message.Should().BeEquivalentTo(message);
            return this;
        }
    }
}
