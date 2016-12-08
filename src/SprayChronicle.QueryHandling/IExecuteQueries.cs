namespace SprayChronicle.QueryHandling
{
    public interface IExecuteQueries
    {
        bool Executes(object query);

        object Execute(object query);
    }
}
