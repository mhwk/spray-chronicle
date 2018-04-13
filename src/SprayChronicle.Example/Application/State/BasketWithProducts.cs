using System;
using System.Collections.Generic;

namespace SprayChronicle.Example.Application.State
{
    public sealed class BasketWithProducts
    {
        public string BasketId { get; }
            
        public DateTime PickedUpAt { get; }

        public readonly List<string> ProductIds = new List<string>();

        public BasketWithProducts(string basketId, DateTime pickedUpAt)
        {
            BasketId = basketId;
            PickedUpAt = pickedUpAt;
        }

        public BasketWithProducts AddProductId(string productId)
        {
            ProductIds.Add(productId);
            return this;
        }

        public BasketWithProducts RemoveProductId(string productId)
        {
            ProductIds.Remove(productId);
            return this;
        }
    }
}
