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
        var users = await _userManager.Users
            .OrderBy(x => x.UserName)
            .ToListAsync();

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
        {
            ModelState.AddModelError(
                nameof(model.Role),
                "Выбрана неизвестная роль.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(
            user, model.Password);

        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty, error.Description);
            }

            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user, model.Role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty, error.Description);
            }

            return View(model);
        }

        TempData["SuccessMessage"] =
            $"Пользователь «{user.UserName}» создан.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);

        if (user.Id == currentUserId && user.IsActive)
        {
            TempData["ErrorMessage"] =
                "Нельзя заблокировать собственную учётную запись.";

            return RedirectToAction(nameof(Index));
        }

        if (user.IsActive)
        {
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains(AppRoles.Administrator))
            {
                var activeUsers = await _userManager.Users
                    .Where(x => x.IsActive)
                    .ToListAsync();

                var activeAdminCount = 0;

                foreach (var activeUser in activeUsers)
                {
                    var activeRoles =
                        await _userManager.GetRolesAsync(activeUser);

                    if (activeRoles.Contains(AppRoles.Administrator))
                    {
                        activeAdminCount++;
                    }
                }

                if (activeAdminCount <= 1)
                {
                    TempData["ErrorMessage"] =
                        "Нельзя заблокировать последнего активного администратора.";

                    return RedirectToAction(nameof(Index));
                }
            }
        }

        user.IsActive = !user.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(
                "; ",
                updateResult.Errors.Select(x => x.Description));
        }
        else
        {
            TempData["SuccessMessage"] = user.IsActive
                ? $"Пользователь «{user.UserName}» разблокирован."
                : $"Пользователь «{user.UserName}» заблокирован.";
        }

        return RedirectToAction(nameof(Index));
    }
}
