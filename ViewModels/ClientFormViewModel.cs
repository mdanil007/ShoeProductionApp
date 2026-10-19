using System.ComponentModel.DataAnnotations;
using ShoeProductionApp.Entities;
namespace ShoeProductionApp.ViewModels;

public class ClientFormViewModel
{
    public int Id { get; set; }
    public string PageTitle { get; set; } = "Клиент";
    [Required(ErrorMessage = "Введите название клиента.")]
    [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов.")]
    [Display(Name = "Название клиента")]
    public string Name { get; set; } = string.Empty;
    [EnumDataType(typeof(ClientType), ErrorMessage = "Выберите корректный тип клиента.")]
    [Display(Name = "Тип клиента")]
    public ClientType Type { get; set; } = ClientType.Shop;
    [StringLength(200, ErrorMessage = "Имя контактного лица не должно превышать 200 символов.")]
    [Display(Name = "Контактное лицо")]
    public string? ContactPerson { get; set; }
    [StringLength(50, ErrorMessage = "Телефон не должен превышать 50 символов.")]
    [Display(Name = "Телефон")]
    public string? Phone { get; set; }
    [EmailAddress(ErrorMessage = "Введите корректный адрес электронной почты.")]
    [StringLength(254, ErrorMessage = "Электронная почта не должна превышать 254 символа.")]
    [Display(Name = "Электронная почта")]
    public string? Email { get; set; }
    [StringLength(500, ErrorMessage = "Адрес не должен превышать 500 символов.")]
    [Display(Name = "Адрес")]
    public string? Address { get; set; }
    [StringLength(2000, ErrorMessage = "Комментарий не должен превышать 2000 символов.")]
    [Display(Name = "Комментарий")]
    public string? Comment { get; set; }
}
