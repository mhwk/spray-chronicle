using System;
using Autofac;
using Autofac.Core;
using Xunit;

namespace SprayChronicle.Testing
{
    public abstract class ProjectionQueryTestCase<TModule> where TModule : IModule, new()
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
        public virtual void ItAcceptsScenario()
        {
            new ProjectionQueryFixture<TModule>(Configure)
                .Given(Given())
                .When(When())
                .ExpectException(ExpectException())
                .Expect(Expect());
        }
    }
}