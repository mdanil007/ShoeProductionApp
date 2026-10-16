using ShoeProductionApp.Entities;

namespace ShoeProductionApp.ViewModels;

public class OrderStatusHistoryItemViewModel
{
    public OrderStatus Status { get; set; }

    public DateTimeOffset ChangedAt { get; set; }

    public string ChangedByUserName { get; set; } = string.Empty;
}
