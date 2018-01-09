using System;

namespace SprayChronicle.Testing
{
    public class TestException : Exception
    {
        public TestException(string message)
            : base(message)
        {
        }
    }
}