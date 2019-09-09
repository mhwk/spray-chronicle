using System;

namespace SprayChronicle
{
    public class ProjectionUnsupportedException : ArgumentException
    {
        public ProjectionUnsupportedException(Projection projection) :
            base($"Operation {projection.GetType()} is not supported")
        {
        }
    }
}
