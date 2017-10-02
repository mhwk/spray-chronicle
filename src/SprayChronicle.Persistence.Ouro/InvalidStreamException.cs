namespace SprayChronicle.Persistence.Ouro
{
    public class InvalidStreamException : OuroException
    {
        public InvalidStreamException(string message): base(message)
        {}
    }
}