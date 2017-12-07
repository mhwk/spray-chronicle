using System;
using System.Threading.Tasks;

namespace SprayChronicle.Testing
{
    public interface IPopulate<TPopulate,TPopulateResult,TExecute,TValidate>
        where TPopulate : class
        where TPopulateResult : class
        where TExecute : class
        where TValidate : class
    {
		Task<IExecute<TExecute,TValidate>> Given(Func<TPopulate,TPopulateResult> callback);
    }
}
