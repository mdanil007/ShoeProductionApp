namespace ShoeProductionApp.Entities
{
    public class Color
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public ICollection<Model> Models { get; set; } = new List<Model>();
    }
}
