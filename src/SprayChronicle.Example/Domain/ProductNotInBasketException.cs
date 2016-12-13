using System;

namespace SprayChronicle.Example.Domain
{
    public class ProductNotInBasketException : Exception
    {
        public ProductNotInBasketException(string message): base(message)
        {}
    }
}
