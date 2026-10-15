namespace ShoeProductionApp.Entities
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public OrderStatus Status { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
        public string ChangedByUserId { get; set; } = string.Empty;
        public ApplicationUser ChangedByUser { get; set; } = null!;
    }
}
