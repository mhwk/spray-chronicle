
namespace SprayChronicle.Example.Domain
{
    public abstract class Identifier
    {
        readonly string _id;

        public Identifier(string id)
        {
            _id = id;
        }

        public override string ToString()
        {
            return _id;
        }
    }
}
