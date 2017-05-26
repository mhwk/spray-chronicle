namespace SprayChronicle.CommandHandling
{
    public interface IDispatchCommand
    {
        void Dispatch(object command);
    }
}
