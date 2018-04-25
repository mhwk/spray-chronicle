using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprayChronicle.EventHandling
{
    public sealed class PipelineManager : IPipelineManager
    {
        private Task _running;

        private readonly List<IPipeline> _pipelines = new List<IPipeline>();

        public PipelineManager()
        {
            
        }

        public IPipelineManager Attach(IPipeline pipeline)
        {
            _pipelines.Add(pipeline);

            return this;
        }
        
        public Task Start()
        {
            if (null != _running) {
                throw new Exception("Pipeline manager already running");
            }

            Console.WriteLine(string.Join(", ", _pipelines.Select(p => p.GetType().Name).ToArray()));
            
//            return Task.WhenAll(_pipelines.Select(p => p.Start()).ToArray());
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _pipelines.ForEach(p => p.Stop());

            return Task.CompletedTask;
        }
    }
}
