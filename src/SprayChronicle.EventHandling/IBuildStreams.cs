namespace SprayChronicle.EventHandling
{
    public interface IBuildStreams
    {
        IStream CatchUp(string reference, string @namespace);

        IStream Persistent(string reference, string category, string @namespace);
    }
}
