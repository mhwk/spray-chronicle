using SprayChronicle.Server.Http;
using System.ComponentModel.DataAnnotations;

namespace SprayChronicle.Example.Contracts.Queries
{
    [HttpQueryAttribute("baskets/pick-ups-on-day")]
    public sealed class PickedUpBasketsOnDay
    {
        [Required]
        public string Day { get; }

        public PickedUpBasketsOnDay(string day)
        {
            Day = day;
        }
    }
}