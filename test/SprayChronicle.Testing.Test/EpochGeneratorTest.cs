using System;
using Shouldly;
using Xunit;

namespace SprayChronicle.Testing.Test
{
    public class EpochGeneratorTest
    {
        [Fact]
        public void AddsIndexOnDemand()
        {
            var generator = new EpochGenerator();
            
            generator[0].ShouldBeAssignableTo<DateTime>();
            generator[1].ShouldBeAssignableTo<DateTime>();
        }

        [Fact]
        public void AddIso8601()
        {
            var generator = new EpochGenerator();
            
            generator.Add("2018-01-13T12:13:14+01:00");
            generator[0].ToString("yyyy-MM-dd HH:mm:ss").ShouldBe("2018-01-13 11:13:14");
        }
    }
}