namespace ShoeProductionApp.Entities
{
    public class Client
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ClientType Type { get; set; }

        public string? ContactPerson { get; set; }
        public string? Phone {  get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        public string? Comment { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
