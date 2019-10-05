using System.Threading.Tasks;

namespace SprayChronicle
{
    public interface IProject
    {
        Task<Projection> Project(Envelope envelope);
    }
}
