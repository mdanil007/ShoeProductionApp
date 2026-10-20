using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using ShoeProductionApp.Entities;

namespace ShoeProductionApp.Middleware;

public class ActiveUserMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager)
    {
        var path = context.Request.Path;

        // Страница входа должна оставаться доступной после выхода.
        if (path.StartsWithSegments("/Identity/Account/Login"))
        {
            await _next(context);
            return;
        }

        // Статические файлы не требуют проверки учётной записи.
        if (path.StartsWithSegments("/css") ||
            path.StartsWithSegments("/js") ||
            path.StartsWithSegments("/lib") ||
            path.StartsWithSegments("/images") ||
            path.Equals(new PathString("/favicon.ico")))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = userManager.GetUserId(context.User);
            var user = userId is null
                ? null
                : await userManager.FindByIdAsync(userId);

            if (user is null || !user.IsActive)
            {
                await SignOutAndRedirectAsync(context, "blocked");
                return;
            }

            // Сравниваем роль в текущей сессии с ролью в базе данных.
            var actualRoles = await userManager.GetRolesAsync(user);

            var sessionRoles = context.User
                .FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .OrderBy(role => role, StringComparer.Ordinal)
                .ToArray();

            var databaseRoles = actualRoles
                .OrderBy(role => role, StringComparer.Ordinal)
                .ToArray();

            if (!sessionRoles.SequenceEqual(
                    databaseRoles,
                    StringComparer.Ordinal))
            {
                await SignOutAndRedirectAsync(context, "changed");
                return;
            }
        }

        await _next(context);
    }

    private static async Task SignOutAndRedirectAsync(
        HttpContext context,
        string reason)
    {
        await context.SignOutAsync(IdentityConstants.ApplicationScheme);

        context.Response.Redirect(
            $"/Identity/Account/Login?reason={reason}");
    }
}
