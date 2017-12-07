using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using SprayChronicle.EventSourcing;
using FluentAssertions;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;

namespace SprayChronicle.Testing
{
    public class EventSourcedValidator : Validator
    {
        private readonly IContainer _container;

        private readonly IDomainMessage[] _messages;
        
        private readonly Exception _error;
        
        private EventSourcedValidator(IContainer container, IDomainMessage[] messages)
        {
            _container = container;
            _messages = messages;
        }
        
        private EventSourcedValidator(IContainer container, Exception error)
        {
            _container = container;
            _messages = new IDomainMessage[] {};
            _error = error;
        }

        public static async Task<EventSourcedValidator> Run<TSourced>(IContainer container, Func<TSourced> callback)
            where TSourced : EventSourced<TSourced>
        {
            return await Task.Run(() => {
                container.Resolve<TestStore>().Present();
                
                try {
                    return new EventSourcedValidator(
                        container,
                        callback()?.Diff().ToArray()
                    );
                } catch (Exception error) {
                    return new EventSourcedValidator(
                        container,
                        error
                    );
                }
            });
        }
        
		public override IValidate Expect()
        {
            _messages.Should().BeEmpty();
            return this;
        }

		public override IValidate Expect(int count)
        {
            _messages.Should().HaveCount(count);
            return this;
        }

		public override IValidate Expect(params object[] results)
		{
		    var expect = results;
		    var actual = _messages.Select(dm => dm.Payload()).ToArray();
		    
		    actual.ShouldAllBeEquivalentTo(
                results,
                options => options.WithStrictOrdering().RespectingRuntimeTypes(),
                Diff(expect, actual)
            );
            
            return this;
        }

		public override IValidate Expect(params Type[] types)
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

		public override IValidate ExpectNoException()
        {
            _error.Should().BeNull(_error?.ToString());
            return this;
        }

		public override IValidate ExpectException(Type type)
        {
            if (null == type) {
                ExpectNoException();
            } else {
                _error.Should().BeOfType(type, _error?.ToString());
            }
            return this;
        }

		public override IValidate ExpectException(string message)
        {
            _error.Message.Should().BeEquivalentTo(message);
            return this;
        }
    }
}