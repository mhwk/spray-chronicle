using System;
using System.Threading.Tasks;

namespace SprayChronicle.Testing
{
    public interface IExecute<TExecute,TValidate>
        where TExecute : class
        where TValidate : class
    {
        Task<IValidate> When(Func<TExecute,TValidate> callback);
    }
}
