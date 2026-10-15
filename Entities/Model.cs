namespace ShoeProductionApp.Entities
{
    public class Model
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int ColorId { get; set; }
        public Color Color { get; set; } = null!;
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
