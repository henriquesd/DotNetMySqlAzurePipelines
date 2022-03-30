namespace MinimalApiDotNet.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public bool Active { get; set; }
    }
}
