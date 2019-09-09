using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SprayChronicle.Test.Example.Application;
using SprayChronicle.Test.Example.Domain;
using SprayChronicle.Test.Example.Domain.Model;
using Xunit;

namespace SprayChronicle.Test
{
    public class CommandHandlerTest
    {
        private readonly IStoreEvents _events = Substitute.For<IStoreEvents>();
        private readonly IStoreSnapshots _snapshots = Substitute.For<IStoreSnapshots>();

        [Fact]
        public async Task ItShouldStartWithNewSnapshot()
        {
            _snapshots
                .Load<Shopping>("customerId", "messageId")
                .Returns(Task.FromResult(new Snapshot(
                    -1,
                    "customerId",
                    new Shopping()
                )));
            _events
                .Load<Shopping>("customerId", "messageId", -1)
                .Returns(new Envelope<object>[0].ToAsync());

            await _events
                .Append<Shopping>(Arg.Do<IEnumerable<Envelope<object>>>(envelopes => {
                    var first = envelopes.First();
                    first.Sequence.ShouldBe(0);
                    ((ProductChosen) first.Message).ShouldNotBeNull();
                    ((ProductChosen) first.Message).CustomerId.ShouldBe("customerId");
                    ((ProductChosen) first.Message).ProductId.ShouldBe("productId");
                }));
            
            await _snapshots
                .Save<Shopping>(Arg.Do<Snapshot>(snapshot => {
                    snapshot.Sequence.ShouldBe(0);
                    snapshot.Identity.ShouldBe("customerId");
                    snapshot.Snap.ShouldBeOfType<Shopping>();
                }));

            await new CommandHandler<Shopping>(_events, _snapshots, "customerId")
                .Handle(new ChooseProduct("customerId", "productId"), "messageId");
        }

        [Fact]
        public async Task ItShouldContinueWithEvents()
        {
            _snapshots
                .Load<Shopping>("customerId", "messageId")
                .Returns(Task.FromResult(new Snapshot(
                    -1,
                    "customerId",
                    new Shopping()
                )));
            _events
                .Load<Shopping>("customerId", "messageId", -1)
                .Returns(new [] {
                    new Envelope<object>(
                        "customerId",
                        "Shopping",
                        0,
                        new ProductChosen("customerId", "productId")
                    )
                }.ToAsync());
            
            await Should.ThrowAsync<ShouldAssertException>(async () =>
                await new CommandHandler<Shopping>(_events, _snapshots, "customerId")
                    .Handle(new ChooseProduct("customerId", "productId"), "messageId"));
            
            await _events
                .DidNotReceive()
                .Append<Shopping>(Arg.Any<IEnumerable<Envelope<object>>>());
            await _snapshots
                .DidNotReceive()
                .Save<Shopping>(Arg.Any<Snapshot>());
        }
    }
}
