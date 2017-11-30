using System;
using System.Linq;
using Autofac;
using SprayChronicle.EventSourcing;
using FluentAssertions;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;

namespace SprayChronicle.Testing
{
    public class EventSourcedValidator<TSourced> : Validator<TSourced> where TSourced : EventSourced<TSourced>
    {
        private readonly IContainer _container;

        private readonly IDomainMessage[] _messages;
        
        private readonly Exception _error;
        
        public EventSourcedValidator(IContainer container, Func<TSourced> callback)
        {
            _container = container;
            
            try {
                _messages = callback()?.Diff().ToArray();
            } catch (Exception error) {
                _error = error;
                _messages = new IDomainMessage[] {};
            }
        }

		public override IValidate<TSourced> Expect()
        {
            _messages.Should().BeEmpty();
            return this;
        }

		public override IValidate<TSourced> Expect(int count)
        {
            _messages.Should().HaveCount(count);
            return this;
        }

		public override IValidate<TSourced> Expect(params object[] results)
		{
		    Expect(results.Select(r => r.GetType()).ToArray());
		    
		    var expect = results;
		    var actual = _messages.Select(dm => dm.Payload()).ToArray();
		    
		    actual.ShouldAllBeEquivalentTo(
                results,
                options => options.WithStrictOrdering().RespectingRuntimeTypes(),
                Diff(expect, actual)
            );
            
            return this;
        }

		public override IValidate<TSourced> Expect(params Type[] types)
		{
		    var expect = types.Select(type => type.FullName).ToArray();
		    var actual = _messages.Select(dm => dm.Payload().GetType().FullName).ToArray();
		    
		    actual.ShouldAllBeEquivalentTo(
		        expect,
                options => options.WithStrictOrdering().RespectingRuntimeTypes(),
                Diff(expect, actual)
            );
            
            return this;
        }

		public override IValidate<TSourced> ExpectNoException()
        {
            _error.Should().BeNull(_error?.ToString());
            return this;
        }

		public override IValidate<TSourced> ExpectException(Type type)
        {
            if (null == type) {
                ExpectNoException();
            } else {
                _error.Should().BeOfType(type, _error?.ToString());
            }
            return this;
        }

		public override IValidate<TSourced> ExpectException(string message)
        {
            _error.Message.Should().BeEquivalentTo(message);
            return this;
        }
    }
}