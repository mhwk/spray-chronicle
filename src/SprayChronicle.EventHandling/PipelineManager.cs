using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprayChronicle.EventHandling
{
    public sealed class PipelineManager : IPipelineManager
    {
        public string Description => "PipelineManager";

        private Task _running;

        private readonly List<IPipeline> _pipelines = new List<IPipeline>();

        public PipelineManager()
        {
            Attach(this);
        }

        public IPipelineManager Attach(IPipeline pipeline)
        {
            _pipelines.Add(pipeline);

            return this;
        }
        
        public Task Start()
        {
            if (null != _running) {
                throw new Exception("Pipeline manager alreeady running");
            }
            
            return Task.WhenAll(_pipelines.Select(p => p.Start()));
        }

        public Task Stop()
        {
            _pipelines.ForEach(p => p.Stop());

            return Task.CompletedTask;
        }
    }
}
