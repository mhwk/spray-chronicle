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

        protected abstract Task Given(CommandRouter dispatcher);

        protected abstract Task When(CommandRouter dispatcher);

        protected abstract void Then(IValidate validator);

        [Fact]
        public virtual async Task Scenario()
        {
            var fixture = new CommandFixture<TModule>(Configure);

            await fixture.Given(Given);

            Then(await fixture.When(When));
        }
    }
}
