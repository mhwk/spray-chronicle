using System;
using Xunit;
using Moq;
using FluentAssertions;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Contract.Command;

namespace SprayChronicle.Test.CommandHandling
{
    public class OverloadCommandHandlerTest
    {
        public Mock<IObjectRepository<ExampleAggregate>> Repository = new Mock<IObjectRepository<ExampleAggregate>>();

        [Fact]
        public void ItWontAcceptCommand()
        {
            new ExampleCommandHandler(Repository.Object).Handles(new DoNotAcceptExample()).Should().BeFalse();
        }

        [Fact]
        public void IDoesAcceptCommand()
        {
            new ExampleCommandHandler(Repository.Object).Handles(new DoAcceptExample("foo")).Should().BeTrue();
        }

        [Fact]
        public void ItFailsOnUnsupportedCommand()
        {
            Action a = () => new ExampleCommandHandler(Repository.Object).Handle(new DoNotAcceptExample());
            a.ShouldThrow<UnhandledCommandException>();
        }

        [Fact]
        public void ItAcceptsSupportedCommand()
        {
            new ExampleCommandHandler(Repository.Object).Handle(new DoAcceptExample("foo"));
            Repository.Verify(repository => repository.Save(It.IsAny<ExampleAggregate>()));
        }
    }
}
