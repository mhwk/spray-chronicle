namespace SprayChronicle.Persistence.Ouro
{
    public sealed class InvalidStreamException : OuroException
    {
        public InvalidStreamException(string message): base(message)
        {}
    }
}