using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SprayChronicle.Test.Example.Application;

namespace SprayChronicle.Test.Example.Domain.Model
{
    public sealed class Shopping : Invariant<Shopping>
    {
        private readonly ImmutableList<string> _products;

        public Shopping() : this(ImmutableList<string>.Empty)
        {
        }

        public Shopping(ImmutableList<string> products)
        {
            _products = products;
        }
        
        public override Shopping Arrange(object evt)
        {
            switch (evt) {
                default: return this;
                case ProductChosen e:
                    return new Shopping(
                        _products.Add(e.ProductId)
                    );
            }
        }

        public override async Task<Shopping> Act(object cmd)
        {
            switch (cmd) {
                default: throw new CommandInvalidException(cmd);
                case ChooseProduct c:
                    _products.Any(p => p == c.ProductId).ShouldBeFalse("Product already added");
                    return Apply(new ProductChosen(
                        c.CustomerId,
                        c.ProductId
                    ));
            }
        }
    }
}
