using System;
using Xunit;
using FluentAssertions;

namespace SprayChronicle.Persistence.Memory.Test
{
    public class MemoryRepositoryTest
    {
        [Fact]
        public void TestSaveNewImmutable()
        {
            var repository = new MemoryRepository<Test>();
            var test = new Test();
            repository.Save(test);

            test = repository.Load("id").DoFoo();
            repository.Save(test);

            repository.Load("id").Foo.ShouldBeEquivalentTo(true);
        }

        public class Test
        {
            [Identifier]
            public readonly string Id = "id";

            public readonly bool Foo;

            public Test()
            {
                Foo = false;
            }

            private Test(bool foo)
            {
                Foo = foo;
            }

            public Test DoFoo()
            {
                return new Test(true);
            }
        }
    }
}