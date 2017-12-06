namespace SprayChronicle.MessageHandling
{
    public static class ObjectExtensions
    {
        public static IMessage ToMessage(this object obj)
        {
            if (obj is IMessage message) {
                return message;
            }
            
            return new Message(obj);
        }
    }
}