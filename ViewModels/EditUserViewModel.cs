using System.ComponentModel.DataAnnotations;

namespace ShoeProductionApp.ViewModels;

public class EditUserViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Display(Name = "Логин")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите роль.")]
    [Display(Name = "Роль")]
    public string Role { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Пароль должен содержать не менее 8 символов.")]
    [Display(Name = "Новый пароль")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Подтверждение нового пароля")]
    public string? ConfirmPassword { get; set; }
}
