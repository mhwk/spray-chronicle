using SprayChronicle.Server.Http;
using System.ComponentModel.DataAnnotations;

namespace SprayChronicle.Example.Application
{
    [HttpQuery("baskets/picked-up-per-day")]
    public sealed class PickedUpPerDay
    {
        [Required]
        public int Page { get; }

        public PickedUpPerDay(int page = 1)
        {
            Page = page;
        }
    }
}