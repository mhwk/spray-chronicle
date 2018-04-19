using System;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class Checkpoint
    {
        public string Id { get; }
        
        public long Sequence { get; private set; }

        public Checkpoint(string id)
        {
            Id = id;
            Sequence = 0;
        }

        public Checkpoint Increase(long sequence)
        {
            if (sequence < Sequence) {
                throw new ArgumentException($"Checkpoint sequence {sequence} is expected to be equal or higher than {Sequence}");
            }

            Sequence = sequence;
            
            return this;
        }
    }
}
