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
                .Watch(Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
                .Returns(new [] {
                    new Envelope<object>(
                        "invariantId",
                        "Invariant",
                        0,
                        new ChooseProduct("customerId", "productId")
                    )
                }.ToAsync());
            
            var envelope = default(Envelope<object>);
            var processor = new Processor<TestProcess>(_logger, _events, new TestProcess(async e => {
                envelope = e;
                cancellationSource.Cancel();
            }));
            await processor.StartAsync(cancellationSource.Token);
            await completionSource.Task;
            
            envelope.ShouldNotBeNull();
        }

        public class TestProcess : IProcess
        {
            private readonly Func<Envelope<object>, Task> _callback;

            public TestProcess(Func<Envelope<object>, Task> callback)
            {
                _callback = callback;
            }
            
            public async Task Process(Envelope<object> envelope)
            {
                await _callback(envelope);
            }
        }
    }
}
