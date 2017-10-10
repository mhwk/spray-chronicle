using System;

namespace SprayChronicle.Example.Domain.Model
{
    public class ProductNotInBasketException : Exception
    {
        public ProductNotInBasketException(string message): base(message)
        {}
    }
}
