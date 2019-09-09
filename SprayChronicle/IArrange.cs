namespace SprayChronicle
{
    public interface IArrange
    {
        
    }
    
    public interface IArrange<out T> : IArrange
    {
        T Arrange(object evt);
    }
}
