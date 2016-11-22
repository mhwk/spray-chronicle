using System;
using Xunit;
using Autofac;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public abstract class EventSourcedTestCase<THandler,TSource> where THandler : IHandleCommand where TSource: EventSourced<TSource>
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
        public void ItAcceptsScenario()
        {
            var container = new ContainerBuilder();
            Configure(container);
            
            new EventSourcedFixture<THandler,TSource>(container.Build())
                .Given(Given())
                .When(When())
                .ExpectException(ExpectException())
                .Expect(Expect());
        }
    }
}
