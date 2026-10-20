using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeProductionApp.Authorization;
using ShoeProductionApp.Entities;
using ShoeProductionApp.ViewModels;

namespace ShoeProductionApp.Controllers;

[Authorize(Policy = "CanManageUsers")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderBy(x => x.UserName).ToListAsync();
        var result = new List<UserListItemViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserListItemViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Role = roles.SingleOrDefault() ?? "Роль не назначена",
                IsActive = user.IsActive
            });
        }

        return View(result);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Roles = AppRoles.All;
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        ViewBag.Roles = AppRoles.All;
        model.UserName = model.UserName?.Trim() ?? string.Empty;

        if (!AppRoles.All.Contains(model.Role))
            ModelState.AddModelError(nameof(model.Role), "Выбрана неизвестная роль.");

        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser { UserName = model.UserName, IsActive = true };
        var createResult = await _userManager.CreateAsync(user, model.Password);

        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            AddIdentityErrors(roleResult);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Пользователь «{user.UserName}» создан.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        ViewBag.Roles = AppRoles.All;
        return View(new EditUserViewModel
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Role = roles.SingleOrDefault() ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        ViewBag.Roles = AppRoles.All;

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user is null) return NotFound();

        // The username is read-only and is never changed by this action.
        model.UserName = user.UserName ?? string.Empty;

        if (!AppRoles.All.Contains(model.Role))
            ModelState.AddModelError(nameof(model.Role), "Выбрана неизвестная роль.");

        var changingPassword = !string.IsNullOrWhiteSpace(model.NewPassword) ||
                               !string.IsNullOrWhiteSpace(model.ConfirmPassword);

        if (changingPassword)
        {
            if (string.IsNullOrWhiteSpace(model.NewPassword))
                ModelState.AddModelError(nameof(model.NewPassword), "Введите новый пароль.");
            if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Подтвердите новый пароль.");
            else if (model.NewPassword != model.ConfirmPassword)
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Пароли не совпадают.");
        }

        // Validate the new password before changing any account data, so a password-policy
        // error cannot leave the role changed while the form reports failure.
        if (changingPassword && !string.IsNullOrWhiteSpace(model.NewPassword))
        {
            foreach (var validator in _userManager.PasswordValidators)
            {
                var passwordValidation = await validator.ValidateAsync(_userManager, user, model.NewPassword);
                if (!passwordValidation.Succeeded)
                    AddIdentityErrors(passwordValidation);
            }
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentlyAdmin = currentRoles.Contains(AppRoles.Administrator);
        var willBeAdmin = model.Role == AppRoles.Administrator;

        if (user.IsActive && currentlyAdmin && !willBeAdmin &&
            !await HasAnotherActiveAdministratorAsync(user.Id))
        {
            ModelState.AddModelError(nameof(model.Role),
                "Нельзя снять роль администратора с последнего активного администратора.");
        }

        if (!ModelState.IsValid)
            return View(model);

        if (!string.Equals(currentRoles.SingleOrDefault(), model.Role, StringComparison.Ordinal))
        {
            if (currentRoles.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    AddIdentityErrors(removeResult);
                    return View(model);
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addResult.Succeeded)
            {
                // Try to restore the original role if the new role could not be assigned.
                if (currentRoles.Count > 0)
                    await _userManager.AddToRolesAsync(user, currentRoles);
                AddIdentityErrors(addResult);
                return View(model);
            }
        }

        if (changingPassword)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword!);
            if (!passwordResult.Succeeded)
            {
                AddIdentityErrors(passwordResult);
                return View(model);
            }
        }

        TempData["SuccessMessage"] = changingPassword
            ? $"Данные пользователя «{user.UserName}» и пароль обновлены."
            : $"Данные пользователя «{user.UserName}» обновлены.";

        // If the current administrator changed their own role, they may no longer have access to this page.
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var currentUserId = _userManager.GetUserId(User);
        if (user.Id == currentUserId && user.IsActive)
        {
            TempData["ErrorMessage"] = "Нельзя заблокировать собственную учётную запись.";
            return RedirectToAction(nameof(Index));
        }

        if (user.IsActive)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(AppRoles.Administrator) &&
                !await HasAnotherActiveAdministratorAsync(user.Id))
            {
                TempData["ErrorMessage"] = "Нельзя заблокировать последнего активного администратора.";
                return RedirectToAction(nameof(Index));
            }
        }

        user.IsActive = !user.IsActive;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join("; ", updateResult.Errors.Select(x => x.Description));
        }
        else
        {
            TempData["SuccessMessage"] = user.IsActive
                ? $"Пользователь «{user.UserName}» разблокирован."
                : $"Пользователь «{user.UserName}» заблокирован.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> HasAnotherActiveAdministratorAsync(string excludedUserId)
    {
        var activeUsers = await _userManager.Users
            .Where(x => x.IsActive && x.Id != excludedUserId)
            .ToListAsync();

        foreach (var activeUser in activeUsers)
        {
            var roles = await _userManager.GetRolesAsync(activeUser);
            if (roles.Contains(AppRoles.Administrator)) return true;
        }

        return false;
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            var message = error.Code switch
            {
                "DuplicateUserName" =>
                    "Пользователь с таким логином уже существует.",

                "DuplicateEmail" =>
                    "Пользователь с таким адресом электронной почты уже существует.",

                "PasswordTooShort" =>
                    "Пароль слишком короткий.",

                "PasswordRequiresNonAlphanumeric" =>
                    "Пароль должен содержать хотя бы один специальный символ.",

                "PasswordRequiresDigit" =>
                    "Пароль должен содержать хотя бы одну цифру.",

                "PasswordRequiresLower" =>
                    "Пароль должен содержать хотя бы одну строчную букву.",

                "PasswordRequiresUpper" =>
                    "Пароль должен содержать хотя бы одну заглавную букву.",

                _ => error.Description
            };

            ModelState.AddModelError(string.Empty, message);
        }
    }

}
