using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using SprayChronicle.Test.Example.Application;
using Xunit;

namespace SprayChronicle.Test
{
    public class ProjectorTest
    {
        private readonly ILogger<TestProject> _logger = Substitute.For<ILogger<TestProject>>();
        private readonly IStoreEvents _events = Substitute.For<IStoreEvents>();

        [Fact]
        public async Task ShouldProjectMessages()
        {
            var cancellationSource = new CancellationTokenSource();
            var completionSource = new TaskCompletionSource<bool>();
            cancellationSource.Token.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), completionSource);

            _events
                .Watch(Arg.Any<long?>(), Arg.Any<CancellationToken>())
                .Returns(new[] {
                    new Envelope(
                        "invariantId1",
                        "Invariant",
                        0,
                        new ChooseProduct("customerId", "productId")
                    ),
                    new Envelope(
                        "invariantId2",
                        "Invariant",
                        0,
                        new ChooseProduct("customerId", "productId")
                    )
                }.ToAsync());

            var envelopes = new List<Envelope>();
            var projections = default(Projection[]);
            
            var processor = new TestProjector(
                _logger,
                _events,
                new TestProject(async e => {
                    envelopes.Add(e);
                }),
                2,
                TimeSpan.FromMilliseconds(100), 
                async results => {
                    projections = results.Select(r => r.Projection).ToArray();
                    cancellationSource.Cancel();
                }
            );
            await processor.ExecuteAsync(cancellationSource.Token);
            await await Task.WhenAny(
                completionSource.Task,
                Task.Delay(10000)
            );

            envelopes.Count.ShouldBe(2);
            projections.ShouldNotBeNull();
        }
        
        [Fact]
        public async Task ShouldCommitBatchAfterTimeout()
        {
            var cancellationSource = new CancellationTokenSource();
            var completionSource = new TaskCompletionSource<bool>();
            cancellationSource.Token.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), completionSource);

            _events
                .Watch(Arg.Any<long?>(), Arg.Any<CancellationToken>())
                .Returns(new[] {
                    new Envelope(
                        "invariantId1",
                        "Invariant",
                        0,
                        new ChooseProduct("customerId", "productId")
                    )
                }.ToAsync());

            var envelopes = new List<Envelope>();
            var projections = default(Projection[]);
            
            var processor = new TestProjector(
                _logger,
                _events,
                new TestProject(async e => {
                    envelopes.Add(e);
                }),
                2,
                TimeSpan.FromMilliseconds(10), 
                async results => {
                    projections = results.Select(r => r.Projection).ToArray();
                    cancellationSource.Cancel();
                }
            );
            await processor.ExecuteAsync(cancellationSource.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(20));
            await await Task.WhenAny(
                completionSource.Task,
                Task.Delay(10000)
            );

            envelopes.Count.ShouldBe(1);
            projections.ShouldNotBeNull();
        }

        public class TestProjector : Projector<TestProject>
        {
            private readonly Func<ProjectionResult[], Task> _callback;

            public TestProjector(
                ILogger<TestProject> logger,
                IStoreEvents events,
                TestProject process,
                int batchSize,
                TimeSpan timeout,
                Func<ProjectionResult[], Task> callback
            ) : base(
                logger,
                events,
                process,
                batchSize,
                timeout
            )
            {
                _callback = callback;
            }

            protected override Task<long> Checkpoint()
            {
                return Task.FromResult(-1L);
            }

            protected override async Task Commit(ProjectionResult[] results)
            {
                await _callback(results);
            }
        }

        public class TestProject : IProject
        {
            private readonly Func<Envelope, Task> _callback;

            public TestProject(Func<Envelope, Task> callback)
            {
                _callback = callback;
            }

            public async Task<Projection> Project(Envelope envelope)
            {
                await _callback(envelope);
                return new Projection.None();
            }
        }
    }
}
