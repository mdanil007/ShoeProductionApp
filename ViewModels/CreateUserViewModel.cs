using System.ComponentModel.DataAnnotations;

namespace ShoeProductionApp.ViewModels;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Введите логин.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "Логин должен содержать от 3 до 100 символов.")]
    [Display(Name = "Логин")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль.")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Пароль должен содержать не менее 8 символов.")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите роль.")]
    [Display(Name = "Роль")]
    public string Role { get; set; } = string.Empty;
}
