using ShoeProductionApp.Entities;
using ShoeProductionApp.ViewModel;

namespace ShoeProductionApp.ViewModels;

public class OrderDetailsViewModel
{
    public int Id { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public string Article { get; set; } = string.Empty;

    public string CreatorName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateOnly PlannedShipmentDate { get; set; }

    public DateTimeOffset? ActualShipmentDate { get; set; }

    public string? Comment { get; set; }

    public OrderStatus Status { get; set; }

    public int TotalPairs { get; set; }

    public List<OrderSizeInputViewModel> Sizes { get; set; } = [];

    public List<OrderStatusHistoryItemViewModel> StatusHistory { get; set; } = [];
}
