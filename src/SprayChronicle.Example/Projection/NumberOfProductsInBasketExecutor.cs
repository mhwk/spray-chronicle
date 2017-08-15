using System;
using System.Collections.Generic;
using System.Linq;
using SprayChronicle.QueryHandling;
using SprayChronicle.Example.Contracts.Queries;

namespace SprayChronicle.Example.Projection
{
    public class NumberOfProductsInBasketExecutor : OverloadQueryExecutor<NumberOfProductsInBasket>
    {
        public NumberOfProductsInBasketExecutor(IStatefulRepository<NumberOfProductsInBasket> repository): base(repository)
        {}

        public object On(NumberOfProductsForBasketId query)
        {
            return _repository.Load(q => q.FirstOrDefault(item => item.BasketId == query.BasketId));
        }

        public object On(NumberOfProductsInBaskets query)
        {
            return _repository.Load(q => q);
        }

        public object On(PagedNumberOfProductsInBasket query)
        {
            return _repository.Load(q => q, query.Page, query.PerPage);
        }
    }
}
