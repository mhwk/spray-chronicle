namespace SprayChronicle.MessageHandling
{
    public static class ObjectExtensions
    {
        public static IMessage ToMessage(this object obj)
        {
            return new Message(obj);
        }
    }
}