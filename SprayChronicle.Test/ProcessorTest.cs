using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using SprayChronicle.Test.Example.Application;
using Xunit;

namespace SprayChronicle.Test
{
    public class ProcessorTest
    {
        private readonly IStoreEvents _events = Substitute.For<IStoreEvents>();
        private readonly ILogger<TestProcess> _logger = Substitute.For<ILogger<TestProcess>>();
        
        [Fact]
        public async Task ItShouldProcessMessage()
        {
            var cancellationSource = new CancellationTokenSource();
            var completionSource = new TaskCompletionSource<bool>();
            cancellationSource.Token.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), completionSource);
            
            _events
                .Watch(Arg.Any<Checkpoint?>(), Arg.Any<CancellationToken>())
                .Returns(new [] {
                    new Envelope(
                        "invariantId",
                        "Invariant",
                        0,
                        new ChooseProduct("customerId", "productId")
                    )
                }.ToAsync());
            
            var envelope = default(Envelope);
            var processor = new Processor<TestProcess>(_logger, _events, new TestProcess(async e => {
                envelope = e;
                cancellationSource.Cancel();
            }));
            await processor.ExecuteAsync(cancellationSource.Token);
            await completionSource.Task;
            
            envelope.ShouldNotBeNull();
        }

        public class TestProcess : IProcess
        {
            private readonly Func<Envelope, Task> _callback;

            public TestProcess(Func<Envelope, Task> callback)
            {
                _callback = callback;
            }
            
            public async Task Process(Envelope envelope)
            {
                await _callback(envelope);
            }
        }
    }
}
