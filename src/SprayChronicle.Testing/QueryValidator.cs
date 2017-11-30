using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;

namespace SprayChronicle.Testing
{
    public class QueryValidator : Validator<Task<object>>
    {
        private readonly IContainer _container;
        
        private readonly Exception _error;

        private readonly object[] _projections;

        public QueryValidator(IContainer container, Func<Task<object>> action)
        {
            _container = container;
            
            try {
                var result = action().Result;
                if (result is IEnumerable<object>) {
                    _projections = (result as IEnumerable<object>).ToArray();
                } else {
                    _projections = new [] { result };
                }
            } catch (Exception error) {
                _error = error;
                _projections = new object[] { };
            }
        }

		public override IValidate<Task<object>> Expect()
        {
            _projections.Should().BeEmpty(Diff(_projections, null));
            return this;
        }

		public override IValidate<Task<object>> Expect(int count)
        {
            _projections.Should().HaveCount(count);
            return this;
        }

		public override IValidate<Task<object>> Expect(params object[] results)
		{
		    if (null == results) {
		        return Expect();
		    }
		    
		    Expect(results.Select(r => r.GetType()).ToArray());
            
            _projections.ShouldAllBeEquivalentTo(results, Diff(_projections, results));
		    
            return this;
        }

		public override IValidate<Task<object>> Expect(params Type[] types)
		{
		    var expect = types.Select(t => t.AssemblyQualifiedName).ToArray();
		    var actual = _projections.Select(p => p.GetType().AssemblyQualifiedName).ToArray();
		    
		    actual.ShouldAllBeEquivalentTo(expect, Diff(expect, actual));
		    
            return this;
        }

		public override IValidate<Task<object>> ExpectNoException()
        {
            _error.Should().BeNull(_error?.ToString());
            return this;
        }

		public override IValidate<Task<object>> ExpectException(Type type)
        {
            if (null == type) {
                return ExpectNoException();
            }
            
            _error.Should().BeOfType(type, _error?.ToString());
            return this;
        }

		public override IValidate<Task<object>> ExpectException(string message)
        {
            _error.Message.Should().BeEquivalentTo(message);
            return this;
        }
    }
}
