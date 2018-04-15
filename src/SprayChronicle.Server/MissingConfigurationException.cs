namespace SprayChronicle.Server
{
    public sealed class MissingConfigurationException : ChronicleServerException
    {
        public MissingConfigurationException(string message) : base(message)
        {
        }
    }
}
