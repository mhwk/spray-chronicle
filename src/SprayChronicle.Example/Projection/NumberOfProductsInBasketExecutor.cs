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

        public NumberOfProductsInBasket On(NumberOfProductsForBasketId query)
        {
            return _repository.Query()
                .Where(item => item.BasketId == query.BasketId)
                .FirstOrDefault();
        }

        public IEnumerable<NumberOfProductsInBasket> On(NumberOfProductsInBaskets query)
        {
            return _repository.Query();
        }
    }
}
