using System.Collections;
using System.Threading.Tasks;

namespace SprayChronicle.QueryHandling
{
    public class QueryMetadata
    {
        
    }
    
    public class QueryMetadata<TSession> : QueryMetadata
    {
        public delegate TReturn DoQuery<out TReturn>(TSession storage);
        
        public DoQuery<Task<object>> FirstOrDefault;

        public DoQuery<Task<IEnumerable>> ToList;
    }
}
