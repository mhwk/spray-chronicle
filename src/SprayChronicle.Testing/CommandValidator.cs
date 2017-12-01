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
    public class CommandValidator : Validator
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
            container.Resolve<TestStore>().Record();
            
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

		public override IValidate Expect()
        {
            _container.Resolve<TestStore>().Chronicle().Should().BeEmpty();
            return this;
        }

		public override IValidate Expect(int count)
        {
            _container.Resolve<TestStore>().Chronicle().Should().HaveCount(count);
            return this;
        }

		public override IValidate Expect(params object[] results)
		{
		    var expect = results;
		    var actual = _container.Resolve<TestStore>().Chronicle().Select(dm => dm.Payload()).ToArray();
		    
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
		    var actual = _container.Resolve<TestStore>().Chronicle().Select(dm => dm.Payload().GetType().FullName).ToArray();
		    
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