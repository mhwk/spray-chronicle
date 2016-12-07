namespace SprayChronicle.QueryHandling
{
    public interface IProcessQueries
    {
        bool Processes(object query);

        object Process(object query);
    }
}
