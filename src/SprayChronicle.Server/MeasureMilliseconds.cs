using System;
using System.Diagnostics;

namespace SprayChronicle.Server
{
    public sealed class MeasureMilliseconds : IMeasure
    {
        private readonly DateTime _start;
        private readonly DateTime _stop;

        public MeasureMilliseconds() : this(DateTime.Now)
        {
        }

        private MeasureMilliseconds(DateTime start)
        {
            _start = start;
        }

        private MeasureMilliseconds(DateTime start, DateTime stop)
        {
            _start = start;
            _stop = stop;
        }

        public IMeasure Start()
        {
            return new MeasureMilliseconds(DateTime.Now);
        }

        public IMeasure Stop()
        {
            return new MeasureMilliseconds(_start, DateTime.Now);
        }

        public override string ToString()
        {
            Debug.Assert(null != _start);
            Debug.Assert(null != _stop);

            return string.Format(
                "{0}ms",
                (_stop - _start).Milliseconds
            );
        }
    }
}
