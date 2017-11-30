using System;

namespace SprayChronicle.Testing
{
    public interface IPopulateEpoch<TExecute,TValidate>
        where TExecute : class
        where TValidate : class
    {
		IPopulate<TExecute,TValidate> Epoch(params DateTime[] epochs);
    }
}