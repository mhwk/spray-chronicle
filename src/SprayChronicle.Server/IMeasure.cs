namespace SprayChronicle.Server
{
    public interface IMeasure
    {
        IMeasure Start();

        IMeasure Stop();

        string ToString();
    }
}
