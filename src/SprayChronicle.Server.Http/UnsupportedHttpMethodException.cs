namespace SprayChronicle.Server.Http
{
    public sealed class UnsupportedHttpMethodException : HttpServerException
    {
        public UnsupportedHttpMethodException(string message) : base(message)
        {
        }
    }
}