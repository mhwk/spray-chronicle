namespace SprayChronicle.Persistence.Ouro
{
    public sealed class ProjectionRef
    {
        public string Name { get; }
        public string Status { get; }

        public ProjectionRef(string name, string status)
        {
            Name = name;
            Status = status;
        }
    }
}