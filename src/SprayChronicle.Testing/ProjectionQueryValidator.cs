using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace SprayChronicle.Testing
{
    public class ProjectionQueryValidator : IValidate
    {
        readonly Exception _error;

        readonly IEnumerable<object> _projections;

        public ProjectionQueryValidator(Exception error, IEnumerable<object> projections)
        {
            _error = error;
            _projections = projections;
        }

        public ProjectionQueryValidator(Exception error, params object[] projections)
        {
            _error = error;
            _projections = projections.ToList();
        }

		public IValidate Expect()
        {
            _projections.Should().BeEmpty();
            return this;
        }

		public IValidate Expect(int count)
        {
            _projections.Should().HaveCount(count);
            return this;
        }

		public IValidate Expect(params object[] results)
        {
            _projections.ToArray().ShouldAllBeEquivalentTo(results);
            return this;
        }

		public IValidate Expect(params Type[] types)
        {
            _projections.Select(p => p.GetType()).ShouldAllBeEquivalentTo(types);
            return this;
        }

		public IValidate ExpectNoException()
        {
            _error.Should().BeNull();
            return this;
        }

		public IValidate ExpectException(Type type)
        {
            if (null == type) {
                ExpectNoException();
            } else {
                _error.Should().BeOfType(type, _error.ToString());
            }
            return this;
        }

		public IValidate ExpectException(string message)
        {
            _error.Message.Should().BeEquivalentTo(message);
            return this;
        }
    }
}
