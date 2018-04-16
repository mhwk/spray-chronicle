using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public delegate Task<object> HandleMessage(params object[] arguments);
}
