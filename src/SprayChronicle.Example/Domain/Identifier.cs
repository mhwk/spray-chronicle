
namespace SprayChronicle.Example.Domain
{
    public abstract class Identifier
    {
        readonly string _id;

        public Identifier(string id)
        {
            _id = id;
        }

        public override bool Equals (object obj)
        {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            
            return _id.Equals(((Identifier)obj)._id);
        }
        
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override string ToString()
        {
            return _id;
        }

        public static implicit operator string(Identifier identifier)
        {
            if (null == identifier) {
                return null;
            }
            return identifier._id;
        }
    }
}
