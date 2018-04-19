using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using SprayChronicle.QueryHandling;
using Xunit;

namespace SprayChronicle.Testing
{
    public abstract class QueryTestCase<TModule,TTarget>
        where TModule : IModule, new()
        where TTarget : class
    {
        protected virtual void Configure(ContainerBuilder builder)
        {}

        protected abstract Task Given(TestSource<TTarget> source);

        protected abstract Task<object> When(QueryRouter processor);

        protected abstract void Then(IValidate validate);

        [Fact]
        public virtual async Task Scenario()
        {
            Then(await (await new QueryFixture<TModule,TTarget>(Configure).Given(Given)).When(When));
        }
    }
}