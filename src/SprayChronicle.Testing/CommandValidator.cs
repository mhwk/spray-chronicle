using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using SprayChronicle.EventSourcing;
using FluentAssertions;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;

namespace SprayChronicle.Testing
{
    public class CommandValidator : Validator<Task>
    {
        private readonly IContainer _container;
        
        private readonly Exception _error;
        
        public CommandValidator(IContainer container, Action action)
        {
            _container = container;
            _container.Resolve<TestStore>().Record();
            try {
                action();
            } catch (Exception error) {
                _error = error;
            }
        }

		public override IValidate<Task> Expect()
        {
            _container.Resolve<TestStore>().Chronicle().Should().BeEmpty();
            return this;
        }

		public override IValidate<Task> Expect(int count)
        {
            _container.Resolve<TestStore>().Chronicle().Should().HaveCount(count);
            return this;
        }

		public override IValidate<Task> Expect(params object[] results)
		{
		    Expect(results.Select(r => r.GetType()).ToArray());
		    
		    var expect = results;
		    var actual = _container.Resolve<TestStore>().Chronicle().Select(dm => dm.Payload()).ToArray();
		    
		    actual.ShouldAllBeEquivalentTo(
                results,
                options => options.WithStrictOrdering().RespectingRuntimeTypes(),
                Diff(expect, actual)
            );
            
            return this;
        }

		public override IValidate<Task> Expect(params Type[] types)
		{
		    var expect = types.Select(type => type.FullName).ToArray();
		    var actual = _container.Resolve<TestStore>().Chronicle().Select(dm => dm.Payload().GetType().FullName).ToArray();
		    
		    actual.ShouldAllBeEquivalentTo(
		        expect,
                options => options.WithStrictOrdering().RespectingRuntimeTypes(),
                Diff(expect, actual)
            );
            
            return this;
        }

		public override IValidate<Task> ExpectNoException()
        {
            _error.Should().BeNull(_error?.ToString());
            return this;
        }

		public override IValidate<Task> ExpectException(Type type)
        {
            if (null == type) {
                ExpectNoException();
            } else {
                _error.Should().BeOfType(type, _error?.ToString());
            }
            return this;
        }

		public override IValidate<Task> ExpectException(string message)
        {
            _error.Message.Should().BeEquivalentTo(message);
            return this;
        }
    }
}