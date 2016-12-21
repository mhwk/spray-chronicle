using System;

namespace SprayChronicle.Testing
{
    public interface IPopulateEpoch
    {
		IPopulate Epoch(params DateTime[] epochs);
    }
}