using System.Threading.Tasks;
using Xunit;
using Autofac;
using Autofac.Core;
using SprayChronicle.CommandHandling;

namespace SprayChronicle.Testing
{
    public abstract class HandlingTestCase<TModule> where TModule : IModule, new()
    {
        protected virtual void Configure(ContainerBuilder builder)
        {}

        protected abstract Task Given(ICommandDispatcher dispatcher);

        protected abstract Task When(ICommandDispatcher dispatcher);

        protected abstract void Then(IValidate validator);

        [Fact]
        public virtual async Task Scenario()
        {
            var fixture = new HandlingFixture<TModule>(Configure);

            await fixture.Given(Given);

            Then(await fixture.When(When));
        }
    }
}
