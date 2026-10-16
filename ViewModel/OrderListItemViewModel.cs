using ShoeProductionApp.Entities;

namespace ShoeProductionApp.ViewModels;

public class OrderListItemViewModel
{
    public int Id { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public string Article { get; set; } = string.Empty;

    public int TotalPairs { get; set; }

    public OrderStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateOnly PlannedShipmentDate { get; set; }

    public DateTimeOffset? ActualShipmentDate { get; set; }

    public bool IsOverdue { get; set; }
}
