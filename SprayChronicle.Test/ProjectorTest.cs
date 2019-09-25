using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SprayChronicle.Test.Example.Application;
using Xunit;

namespace SprayChronicle.Test
{
    public class ProjectorTest
    {
        private readonly IStoreEvents _events = Substitute.For<IStoreEvents>();
        private readonly IStoreSnapshots _snapshots = Substitute.For<IStoreSnapshots>();

        [Fact]
        public async Task ShouldProjectMessages()
        {
            var cancellationSource = new CancellationTokenSource();
            var completionSource = new TaskCompletionSource<bool>();
            cancellationSource.Token.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), completionSource);

            _events
                .Watch(Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
                .Returns(new[] {
                    new Envelope<object>(
                        "invariantId1",
                        "Invariant",
                        0,
                        new ChooseProduct("customerId", "productId")
                    ),
                    new Envelope<object>(
                        "invariantId2",
                        "Invariant",
                        0,
                        new ChooseProduct("customerId", "productId")
                    )
                }.ToAsync());

            var envelopes = new List<Envelope<object>>();
            var projections = default(Projection[]);
            
            var processor = new TestProjector(
                _events,
                _snapshots,
                new TestProject(async e => {
                    envelopes.Add(e);
                }),
                2,
                TimeSpan.FromMilliseconds(100), 
                async p => {
                    projections = p;
                    cancellationSource.Cancel();
                }
            );
            await processor.StartAsync(cancellationSource.Token);
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
                .Watch(Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
                .Returns(new[] {
                    new Envelope<object>(
                        "invariantId1",
                        "Invariant",
                        0,
                        new ChooseProduct("customerId", "productId")
                    )
                }.ToAsync());

            var envelopes = new List<Envelope<object>>();
            var projections = default(Projection[]);
            
            var processor = new TestProjector(
                _events,
                _snapshots,
                new TestProject(async e => {
                    envelopes.Add(e);
                }),
                2,
                TimeSpan.FromMilliseconds(10), 
                async p => {
                    projections = p;
                    cancellationSource.Cancel();
                }
            );
            await processor.StartAsync(cancellationSource.Token);
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
            private readonly Func<Projection[], Task> _callback;

            public TestProjector(
                IStoreEvents events,
                IStoreSnapshots snapshots,
                TestProject process,
                int batchSize,
                TimeSpan timeout,
                Func<Projection[], Task> callback
            ) : base(
                events,
                snapshots,
                process,
                batchSize,
                timeout
            )
            {
                _callback = callback;
            }

            protected override async Task Commit(Projection[] projections)
            {
                await _callback(projections);
            }
        }

        public class TestProject : IProject
        {
            private readonly Func<Envelope<object>, Task> _callback;

            public TestProject(Func<Envelope<object>, Task> callback)
            {
                _callback = callback;
            }

            public async Task<Projection> Project(Envelope<object> envelope)
            {
                await _callback(envelope);
                return new Projection.None();
            }
        }
    }
}
