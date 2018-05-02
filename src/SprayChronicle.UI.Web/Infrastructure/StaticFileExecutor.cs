using System;
using System.Threading.Tasks;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.UI.Web.Infrastructure
{
    public sealed class StaticFileExecutor : Executor
    {
        private readonly string _fileName;

        public StaticFileExecutor(string fileName)
        {
            _fileName = fileName;
        }

        internal Task<object> Do(string path)
        {
            Console.WriteLine(_fileName);
            Console.WriteLine(path);

            return null;
        }
    }
}
