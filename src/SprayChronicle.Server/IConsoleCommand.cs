using System;
using System.Threading.Tasks;

namespace SprayChronicle.Server
{
    public interface IConsoleCommand
    {
        string Name { get; }
        string Description { get; }
        Func<Task<int>> Execute { get; }
    }
}