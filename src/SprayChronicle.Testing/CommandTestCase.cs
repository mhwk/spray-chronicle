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

        protected abstract Task Given(IDispatchCommands dispatcher);

        protected abstract Task When(IDispatchCommands dispatcher);

        protected abstract void Then(IValidate validator);

        [Fact]
        public virtual async Task Scenario()
        {
             Then(await (await new CommandFixture<TModule>(Configure).Given(Given)).When(When));
        }
    }
}
