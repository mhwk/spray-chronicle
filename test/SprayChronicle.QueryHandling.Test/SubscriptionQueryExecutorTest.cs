using Xunit;
using Moq;

namespace SprayChronicle.QueryHandling.Test
{
    public class SubscriptionQueryExecutorTest
    {
        Mock<IExecuteQueries> Executor = new Mock<IExecuteQueries>();

        [Fact]
        public void ItFailsIfNoProcessorProcessesQuery()
        {
            Executor.Setup(qp => qp.Executes(It.IsAny<object>())).Returns(false);

            var query = new {};
            var processor = new SubscriptionQueryProcessor();
            processor.AddExecutors(Executor.Object);
            Assert.Throws<UnhandledQueryException>(() => processor.Process(query));
        }

        [Fact]
        public void ItProcessesQuery()
        {
            var query = new {};
            var result = new {};

            Executor.Setup(qp => qp.Executes(It.IsAny<object>())).Returns(true);
            Executor.Setup(qp => qp.Execute(It.IsAny<object>())).Returns(result);

            var processor = new SubscriptionQueryProcessor();
            processor.AddExecutors(Executor.Object);
            Assert.Equal(result, processor.Process(query));

            Executor.Verify(qp => qp.Execute(It.Is<object>(o => o.Equals(query))));
        }
    }
}