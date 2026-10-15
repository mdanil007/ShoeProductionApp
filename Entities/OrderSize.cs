namespace ShoeProductionApp.Entities
{
    public class OrderSize
    {
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public string Size { get; set; } = string.Empty;

        public int Count { get; set; }
    }
}
