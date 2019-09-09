using System;

namespace SprayChronicle
{
    public class CommandInvalidException : ArgumentException
    {
        public CommandInvalidException(object fact) : base($"Unable to handle command {fact.GetType()}")
        {
            
        }
    }
}
