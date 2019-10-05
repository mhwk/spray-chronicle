using System;
using System.Linq;
using System.Threading.Tasks;

namespace SprayChronicle
{
    public sealed class CommandHandler<TInvariant> : IHandleCommand
        where TInvariant : IArrange<TInvariant>, IAct<TInvariant>
    {
        private readonly IStoreEvents _events;
        private readonly IStoreSnapshots _snapshots;
        private readonly string _identity;
        private Snapshot _snapshot;

        public CommandHandler(
            IStoreEvents events,
            IStoreSnapshots snapshots,
            string identity
        ) {
            _events = events;
            _snapshots = snapshots;
            _identity = identity;
        }

        public async Task Handle(object cmd, string messageId = null)
        {
            if (null == _snapshot) {
                _snapshot = await _snapshots.Load<TInvariant>(_identity, messageId);

                await foreach (var envelope in _events.Load<TInvariant>(_identity, messageId, _snapshot.Sequence)) {
                    _snapshot.Sequence++;
                    _snapshot.Snap = ((IArrange<TInvariant>)_snapshot.Snap).Arrange(envelope.Message);
                }
            }

            _snapshot.Snap = await ((IAct<TInvariant>)_snapshot.Snap).Act(cmd);
            
            var envelopes = ((IAct<TInvariant>) _snapshot.Snap)
                .Commit()
                .Select((evt, i) => new Envelope(
                    _identity,
                    typeof(TInvariant).Name,
                    ++_snapshot.Sequence,
                    evt
                ).CausedBy(messageId ?? Guid.NewGuid().ToString()))
                .ToArray();
            
            if (!envelopes.Any())
            {
                return;
            }

            await _events.Append<TInvariant>(envelopes);
            await _snapshots.Save<TInvariant>(_snapshot);
        }
    }
}
