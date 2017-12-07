using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using SprayChronicle.QueryHandling;
using Xunit;

namespace SprayChronicle.Testing
{
    public abstract class QueryTestCase<TModule>
        where TModule : IModule, new()
    {
        protected virtual void Configure(ContainerBuilder builder)
        {}

        protected abstract Task Given(TestStream stream);

        protected abstract Task<object> When(IProcessQueries processor);

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
            (await (await new QueryFixture<TModule>(Configure)
                .Given(Given))
                .When(When))
                .ExpectException(ExpectException())
                .Expect(Expect());
        }
    }
}