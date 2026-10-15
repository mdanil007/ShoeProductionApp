namespace ShoeProductionApp.Entities;

public class Order
{
    public int Id { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string CreatedByUserId { get; set; } = string.Empty;
    public ApplicationUser CreatedByUser { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateOnly PlannedShipmentDate { get; set; }

    public DateTimeOffset? ActualShipmentDate { get; set; }

    public string? Comment { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Created;

    public ICollection<OrderSize> Sizes { get; set; } = new List<OrderSize>();

    public ICollection<OrderStatusHistory> StatusHistory { get; set; }
        = new List<OrderStatusHistory>();
}
