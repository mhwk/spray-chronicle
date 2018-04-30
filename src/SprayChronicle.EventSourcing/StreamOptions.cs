using System;
using System.Linq;

namespace SprayChronicle.EventSourcing
{
    public class StreamOptions
    {
        public string TargetStream { get; }
        
        public string[] Categories { get; }

        public StreamOptions(string targetStream)
            : this(targetStream, new string[0])
        {
        }

        private StreamOptions(string targetStream, string[] categories)
        {
            TargetStream = targetStream;
            Categories = categories;
        }

        public StreamOptions From(params string[] categories)
        {
            return new StreamOptions(TargetStream, categories);
        }

        public StreamOptions From(params Type[] categories)
        {
            return new StreamOptions(TargetStream, categories.Select(c => c.Name).ToArray());
        }
    }
}
