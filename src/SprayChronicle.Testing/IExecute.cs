using System;

namespace SprayChronicle.Testing
{
    public interface IExecute<TExecute,TValidate>
        where TExecute : class
        where TValidate : class
    {
		IValidate<TValidate> When(Func<TExecute,TValidate> callback);
    }
}
