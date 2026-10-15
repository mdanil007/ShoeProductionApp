namespace ShoeProductionApp.Entities
{
    public enum OrderStatus
    {
        Created = 0,
        MaterialCutting = 1,
        Skiving = 2,
        Marking = 3,
        Doubling = 4,
        Labeling = 5,
        UpperAssembly = 6,
        HandLasting = 7,
        Stitching = 8,
        Finishing = 9,
        QualityControl = 10,
        Pachaging = 11,
        Shipping = 12,
        Shipped = 13,
        Cancelled = 14
    }
}
