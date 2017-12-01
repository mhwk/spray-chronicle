using System;
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

        protected virtual object[] Given()
        {
            return new object[] {};
        }

        protected abstract TSourced When(TSourced sourced);

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
            (await new EventSourcedFixture<TModule,TSourced>(Configure)
                .Given(Given())
                .When(When))
                .ExpectException(ExpectException())
                .Expect(Expect());
        }
    }
}
