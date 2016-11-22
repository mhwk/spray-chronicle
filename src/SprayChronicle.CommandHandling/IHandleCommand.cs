namespace SprayChronicle.CommandHandling
{
    public interface IHandleCommand
    {
        bool Handles(object command);

        void Handle(object command);
    }
}
