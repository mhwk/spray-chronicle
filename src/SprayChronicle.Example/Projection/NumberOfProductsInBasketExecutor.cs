using System;
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
            return _repository.FindBy(q => q
                .Where(item => item.BasketId == query.BasketId)
            ).FirstOrDefault();
        }
    }
}
