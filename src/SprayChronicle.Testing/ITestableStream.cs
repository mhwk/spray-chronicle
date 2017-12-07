using System;
using System.Threading.Tasks;
using SprayChronicle.EventHandling;

namespace SprayChronicle.Testing
{
    public interface ITestableStream : IStream
    {
        ITestableStream Epochs(params DateTime[] epochs);
        
        ITestableStream Epochs(params string[] epochs);
        
        Task Publish(params object[] messages);
    }
}
