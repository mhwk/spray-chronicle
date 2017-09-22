using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public sealed class EventSourcedValidator : IValidate
    {
        readonly Exception _error;

        readonly IEnumerable<DomainMessage> _domainMessages;

        public EventSourcedValidator(Exception error, IEnumerable<DomainMessage> domainMessages)
        {
            _error = error;
            _domainMessages = domainMessages;
        }

        public EventSourcedValidator(Exception error, params DomainMessage[] domainMessages)
        {
            _error = error;
            _domainMessages = domainMessages;
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