namespace SprayChronicle.MessageHandling
{
    public interface IMessageHandlingStrategy
    {
        bool AcceptsMessage(object subject, IMessage message, params object[] arguments);

        object ProcessMessage(object subject, IMessage message, params object[] arguments);
    }
}
