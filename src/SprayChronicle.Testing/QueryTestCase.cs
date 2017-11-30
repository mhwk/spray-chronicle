using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using SprayChronicle.QueryHandling;
using Xunit;

namespace SprayChronicle.Testing
{
    public abstract class QueryTestCase<TModule> where TModule : IModule, new()
    {
        protected virtual void Configure(ContainerBuilder builder)
        {}

        protected virtual DateTime[] Epoch()
        {
            return new DateTime[] {};
        }

        protected virtual object[] Given()
        {
            return new object[] {};
        }

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
        public virtual void Scenario()
        {
            new QueryFixture<TModule>(Configure)
                .Epoch(Epoch())
                .Given(Given())
                .When(When)
                .ExpectException(ExpectException())
                .Expect(Expect());
        }
    }
}