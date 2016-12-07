using Xunit;
using Moq;

namespace SprayChronicle.QueryHandling.Test
{
    public class SubscriptionQueryExecutorTest
    {
        Mock<IProcessQueries> QueryProcessor = new Mock<IProcessQueries>();

        [Fact]
        public void ItFailsIfNoProcessorProcessesQuery()
        {
            QueryProcessor.Setup(qp => qp.Processes(It.IsAny<object>())).Returns(false);

            var query = new {};
            var executor = new SubscriptionQueryExecutor();
            executor.AddProcessors(QueryProcessor.Object);
            Assert.Throws<UnhandledQueryException>(() => executor.Execute(query));
        }

        [Fact]
        public void ItProcessesQuery()
        {
            var query = new {};
            var result = new {};

            QueryProcessor.Setup(qp => qp.Processes(It.IsAny<object>())).Returns(true);
            QueryProcessor.Setup(qp => qp.Process(It.IsAny<object>())).Returns(result);

            var executor = new SubscriptionQueryExecutor();
            executor.AddProcessors(QueryProcessor.Object);
            Assert.Equal(result, executor.Execute(query));

            QueryProcessor.Verify(qp => qp.Process(It.Is<object>(o => o.Equals(query))));
        }
    }
}