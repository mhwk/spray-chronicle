using System;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace SprayChronicle.QueryHandling.Test
{
    public class SubscriptionExecutorTest
    {
        private readonly IExecute _executor = Substitute.For<IExecute>();

        [Fact]
        public async Task ItFailsIfNoProcessorProcessesQuery()
        {
            var query = new object();
            
            _executor
                .Executes(Arg.Any<object>())
                .Returns(false);

            await Should.ThrowAsync<UnhandledQueryException>(
                () => new SubscriptionRouter()
                    .Subscribe(_executor)
                    .Route(query)
            );
        }
        [Fact]
        public async Task ItThrowsExecutorError()
        {
            var query = new object();
            
            _executor
                .Execute(Arg.Any<object>())
                .Throws(new Exception("Projection error"));

            await Should.ThrowAsync<Exception>(
                () => new SubscriptionRouter()
                    .Subscribe(_executor)
                    .Route(query)
            );
        }

        [Fact]
        public async Task ItProcessesQuery()
        {
            var query = new object();
            var result = new object();

            _executor
                .Executes(Arg.Any<object>())
                .Returns(true);
            _executor
                .Execute(Arg.Any<object>())
                .Returns(result);

            (await new SubscriptionRouter()
                .Subscribe(_executor)
                .Route(query))
                .ShouldBe(result);

            _executor
                .Received()
                .Execute(Arg.Is(query));
        }
    }
}