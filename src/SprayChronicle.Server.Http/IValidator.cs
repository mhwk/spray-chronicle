namespace SprayChronicle.Server.Http
{
    public interface IValidator
    {
        void Validate(object payload);
    }
}