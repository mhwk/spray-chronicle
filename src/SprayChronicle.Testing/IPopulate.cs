namespace SprayChronicle.Testing
{
    public interface IPopulate<TExecute,TValidate>
        where TExecute : class
        where TValidate : class
    {
		IExecute<TExecute,TValidate> Given(params object[] messages);
    }
}
