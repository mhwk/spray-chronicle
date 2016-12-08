namespace SprayChronicle.MessageHandling
{
    public interface IMessageHandlingStrategy
    {
        bool AcceptsMessage(object subject, object message, params object[] arguments);

        object ProcessMessage(object subject, object message, params object[] arguments);
    }
}
