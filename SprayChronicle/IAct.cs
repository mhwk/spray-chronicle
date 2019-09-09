using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprayChronicle
{
    public interface IAct<TInvariant>
    {
        Task<TInvariant> Act(object cmd);
        IEnumerable<object> Commit();
    }
}
