using System;
using Xunit;
using Autofac;
using Autofac.Core;

namespace SprayChronicle.Testing
{
    public abstract class EventSourcedTestCase<TModule> where TModule : IModule, new()
    {
        protected virtual void Configure(ContainerBuilder builder)
        {}

        protected virtual object[] Given()
        {
            return new object[] {};
        }

        protected abstract object When();

        protected virtual object[] Expect()
        {
            return new object[] {};
        }

        protected virtual Type ExpectException()
        {
            return null;
        }

        [Fact]
        public virtual void Scenario()
        {
            var container = new ContainerBuilder();
            Configure(container);
            
            new EventSourcedFixture<TModule>(Configure)
                .Given(Given())
                .When(When())
                .ExpectException(ExpectException())
                .Expect(Expect());
        }
    }
}
