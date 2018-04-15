namespace SprayChronicle.EventHandling
{
    public interface IPipelineManager : IPipeline
    {
        IPipelineManager Attach(IPipeline pipeline);
    }
}
