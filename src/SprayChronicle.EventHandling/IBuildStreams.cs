namespace SprayChronicle.EventHandling
{
    public interface IBuildStreams
    {
        IStream CatchUp(string reference, ILocateTypes typeLocator);

        IStream Persistent(string reference, string category, ILocateTypes typeLocator);
    }
}
