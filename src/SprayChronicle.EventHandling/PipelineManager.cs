using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprayChronicle.EventHandling
{
    public sealed class PipelineManager : IPipelineManager
    {
        public string Description => "PipelineManager";

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
            return Task.WhenAll(_pipelines.Select(p => p.Start()));
        }
    }
}
