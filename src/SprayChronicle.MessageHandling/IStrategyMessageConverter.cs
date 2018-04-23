namespace SprayChronicle.MessageHandling
{
    public interface IStrategyMessageConverter<TTarget,TFrom,TTo>
    {
        TTo Convert(IMessagingStrategy<TTarget> strategy, TFrom message);
    }
}
