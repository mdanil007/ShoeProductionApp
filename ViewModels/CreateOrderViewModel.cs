using Microsoft.AspNetCore.Mvc.Rendering;
using ShoeProductionApp.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace ShoeProductionApp.ViewModels;

public class CreateOrderViewModel
{
    [Required(ErrorMessage = "Выберите клиента.")]
    public int? ClientId { get; set; }

    [Required(ErrorMessage = "Выберите артикул.")]
    public int? ProductId { get; set; }

    [Required(ErrorMessage = "Укажите плановую дату отгрузки.")]
    [DataType(DataType.Date)]
    public DateOnly? PlannedShipmentDate { get; set; }

    [StringLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов.")]
    public string? Comment { get; set; }

    public List<OrderSizeInputViewModel> Sizes { get; set; } =
    [
        new() { Size = "40", Count = 1 },
        new() { Size = "41", Count = 1 },
        new() { Size = "42", Count = 2 },
        new() { Size = "43", Count = 2 },
        new() { Size = "44", Count = 1 },
        new() { Size = "45", Count = 1 }
    ];

    public List<SelectListItem> Clients { get; set; } = [];
    public List<SelectListItem> Products { get; set; } = [];

    public string MinimumShipmentDate =>
        DateOnly.FromDateTime(
            DateTime.UtcNow.AddHours(3)).ToString("yyyy-MM-dd");
}
