using System;
using System.Collections.Generic;

namespace SprayChronicle.Example.Application.State
{
    public sealed class BasketWithProducts_v1
    {
        public string BasketId { get; }
            
        public DateTime PickedUpAt { get; }

        public readonly List<string> ProductIds = new List<string>();

        public BasketWithProducts_v1(string basketId, DateTime pickedUpAt)
        {
            BasketId = basketId;
            PickedUpAt = pickedUpAt;
        }

        public BasketWithProducts_v1 AddProductId(string productId)
        {
            ProductIds.Add(productId);
            return this;
        }

        public BasketWithProducts_v1 RemoveProductId(string productId)
        {
            ProductIds.Remove(productId);
            return this;
        }
    }
}
