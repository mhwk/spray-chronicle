
namespace SprayChronicle.Example.Domain.Model
{
    public abstract class Identifier
    {
        private readonly string _id;

        protected Identifier(string id)
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
            return identifier?._id;
        }
    }
}
