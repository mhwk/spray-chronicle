namespace SprayChronicle.QueryHandling
{
    public abstract class QueryManager
    {
        private readonly IQueryRouter _router;
        
        

        protected QueryManager(IQueryRouter router)
        {
            _router = router;
        }

        public QueryManager RegisterPipeline(IQueryPipeline pipeline)
        {
            
        }
    }
}
