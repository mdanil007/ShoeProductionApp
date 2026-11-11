namespace ShoeProductionApp.Entities;

public class Product
{
    public int Id { get; set; }
    public Gender Gender { get; set; }

    public int ModelId { get; set; }
    public Model Model { get; set; } = null!;

    public int ColorId { get; set; }
    public Color Color { get; set; } = null!;

    public int LeatherId { get; set; }
    public Leather Leather { get; set; } = null!;

    public int LiningId { get; set; }
    public Lining Lining { get; set; } = null!;

    public int SoleId { get; set; }
    public Sole Sole { get; set; } = null!;

    public string? ImageFileName { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
