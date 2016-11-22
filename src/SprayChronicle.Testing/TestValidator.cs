using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public sealed class TestValidator : IValidate
    {
        readonly IEnumerable<DomainMessage> _domainMessages;

        readonly Exception _error;
        
        public TestValidator(IEnumerable<DomainMessage> domainMessages, Exception error)
        {
            _domainMessages = domainMessages;
            _error = error;
        }

		public IValidate Expect()
        {
            _domainMessages.Should().BeEmpty();
            return this;
        }

		public IValidate Expect(int count)
        {
            _domainMessages.Should().HaveCount(count);
            return this;
        }

		public IValidate Expect(params object[] results)
        {
            _domainMessages.Select(dm => dm.Payload).ShouldAllBeEquivalentTo(results, options => options.WithStrictOrdering().RespectingRuntimeTypes());
            return this;
        }

		public IValidate Expect(params Type[] types)
        {
            _domainMessages.Select(dm => dm.Payload.GetType()).ShouldAllBeEquivalentTo(types);
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
                _error.Should().BeOfType(type);
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