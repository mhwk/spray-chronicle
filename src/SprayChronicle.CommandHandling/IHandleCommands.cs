namespace SprayChronicle.CommandHandling
{
    public interface IHandleCommands
    {
        bool Handles(object command);

        void Handle(object command);
    }
}
