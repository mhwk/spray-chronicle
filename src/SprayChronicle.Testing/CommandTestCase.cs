using System;
using System.Threading.Tasks;
using Xunit;
using Autofac;
using Autofac.Core;
using SprayChronicle.CommandHandling;

namespace SprayChronicle.Testing
{
    public abstract class CommandTestCase<TModule> where TModule : IModule, new()
    {
        protected virtual void Configure(ContainerBuilder builder)
        {}

        protected virtual object[] Given()
        {
            return new object[] {};
        }

        protected abstract Task When(IDispatchCommands dispatcher);

        protected virtual object[] Expect()
        {
            return new object[] {};
        }

        protected virtual Type ExpectException()
        {
            return null;
        }

        [Fact]
        public virtual async Task Scenario()
        {
             (await new CommandFixture<TModule>(Configure)
                .Given(Given())
                .When(When))
                .ExpectException(ExpectException())
                .Expect(Expect());
        }
    }
}