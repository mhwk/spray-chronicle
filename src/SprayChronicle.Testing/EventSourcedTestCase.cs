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

        protected abstract Task<TSourced> Given(TSourced sourced);

        protected abstract Task<TSourced> When(TSourced sourced);

        protected abstract void Then(IValidate validator);

        [Fact]
        public virtual async Task Scenario()
        {
            var fixture = new EventSourcedFixture<TModule, TSourced>(Configure);
            
            await fixture.Given(Given);

            Then(await fixture.When(When));
        }
    }
}
