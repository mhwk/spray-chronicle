namespace SprayChronicle.EventHandling
{
    public interface IBuildStreams
    {
        IStream CatchUp(string reference);

        IStream Persistent(string reference, string category);
    }
}
