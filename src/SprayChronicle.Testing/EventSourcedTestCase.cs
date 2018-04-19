using System.Threading.Tasks;
using Xunit;
using Autofac;
using Autofac.Core;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public abstract class EventSourcedTestCase<TModule,TSourced> where TModule : IModule, new() where TSourced : EventSourced<TSourced>
    {
        protected virtual void Configure(ContainerBuilder builder)
        {}

        protected abstract TSourced Given(TSourced sourced);

        protected abstract TSourced When(TSourced sourced);

        protected abstract Task Then(IValidate validator);

        [Fact]
        public virtual async Task Scenario()
        {
            await Then(await (await new EventSourcedFixture<TModule,TSourced>(Configure).Given(Given)).When(When));
        }
    }
}
