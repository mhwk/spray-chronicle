namespace SprayChronicle.Testing
{
    public interface IPopulate
    {
		IExecute Given(params object[] messages);
    }
}
