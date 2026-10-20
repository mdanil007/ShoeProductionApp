
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeProductionApp.Entities;

namespace ShoeProductionApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ReturnUrl { get; set; } = "/Orders";

    public string? SessionMessage { get; private set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Введите логин.")]
        [Display(Name = "Логин")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }

    public IActionResult OnGet(
        string? returnUrl = null,
        string? reason = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect("/Orders");
        }

        SessionMessage = reason switch
        {
            "blocked" => "Ваша учётная запись заблокирована",
            "changed" => "Ваша учётная запись изменена. Войдите снова",
            _ => null
        };

        ReturnUrl = GetSafeReturnUrl(returnUrl);
        return Page();
    }


    public async Task<IActionResult> OnPostAsync(
        string? returnUrl = null)
    {
        ReturnUrl = GetSafeReturnUrl(returnUrl);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByNameAsync(
            Input.UserName.Trim());

        if (user == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Неверный логин или пароль.");

            return Page();
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(
                string.Empty,
                "Учётная запись заблокирована. Обратитесь к администратору.");

            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return LocalRedirect(ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "Учётная запись временно заблокирована. Попробуйте позже.");

            return Page();
        }

        ModelState.AddModelError(
            string.Empty,
            "Неверный логин или пароль.");

        return Page();
    }

    private string GetSafeReturnUrl(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return returnUrl;
        }

        return "/Orders";
    }
}
