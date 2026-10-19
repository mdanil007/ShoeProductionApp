using ShoeProductionApp.Entities;
namespace ShoeProductionApp.ViewModels;

public class ClientIndexViewModel
{
    public string? Search { get; set; }
    public ClientType? Type { get; set; }
    public List<Client> Clients { get; set; } = [];
}
