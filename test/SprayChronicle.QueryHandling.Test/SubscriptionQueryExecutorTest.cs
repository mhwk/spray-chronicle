using System;
using Xunit;
using Moq;
using FluentAssertions;

namespace SprayChronicle.QueryHandling.Test
{
    public class SubscriptionQueryExecutorTest
    {
        private readonly Mock<IExecuteQueries> _executor = new Mock<IExecuteQueries>();

        [Fact]
        public void ItFailsIfNoProcessorProcessesQuery()
        {
            _executor.Setup(qp => qp.Executes(It.IsAny<object>())).Returns(false);

            var query = new {};
            var processor = new SubscriptionQueryProcessor();
            processor.AddExecutors(_executor.Object);
            Action action = () => processor.Process(query).Wait();
            action.ShouldThrow<UnhandledQueryException>();
        }

        [Fact]
        public void ItProcessesQuery()
        {
            var query = new {};
            var result = new {};

            _executor.Setup(qp => qp.Executes(It.IsAny<object>())).Returns(true);
            _executor.Setup(qp => qp.Execute(It.IsAny<object>())).Returns(result);

            var processor = new SubscriptionQueryProcessor();
            processor.AddExecutors(_executor.Object);
            Assert.Equal(result, processor.Process(query).Result);

            _executor.Verify(qp => qp.Execute(It.Is<object>(o => o.Equals(query))));
        }
    }
}