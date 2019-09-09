
namespace SprayChronicle
{
    public sealed class Snapshot
    {
        public string SnapshotId => $"{Snap.GetType().Name}_{Identity}";
        public long Sequence { get; set; }
        public string Identity { get; set; }
        public object Snap { get; set; }

        public Snapshot(
            long sequence,
            string identity,
            object snap
        )
        {
            Sequence = sequence;
            Identity = identity;
            Snap = snap;
        }
    }
}
