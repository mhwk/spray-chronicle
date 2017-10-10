using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SprayChronicle.QueryHandling;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Model;

namespace SprayChronicle.Example.Application.Service
{
    public class NumberOfProductsInBasketExecutor : OverloadQueryExecutor<NumberOfProductsInBasket>
    {
        public NumberOfProductsInBasketExecutor(IStatefulRepository<NumberOfProductsInBasket> repository): base(repository)
        {}

        public object On(NumberOfProductsForBasketId query)
        {
            Console.WriteLine(JsonConvert.SerializeObject(query));
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