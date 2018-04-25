using System;
using System.Collections.Generic;

namespace SprayChronicle.Example.Application.State
{
    public sealed class BasketWithProducts_v2
    {
        public string Id { get; }
            
        public DateTime PickedUpAt { get; }

        public readonly List<string> ProductIds = new List<string>();

        public BasketWithProducts_v2(string id, DateTime pickedUpAt)
        {
            Id = id;
            PickedUpAt = pickedUpAt;
        }

        public BasketWithProducts_v2 AddProductId(string productId)
        {
            ProductIds.Add(productId);
            return this;
        }

        public BasketWithProducts_v2 RemoveProductId(string productId)
        {
            ProductIds.Remove(productId);
            return this;
        }
    }
}
