using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI;
using ShoeProductionApp.Data;
using ShoeProductionApp.Entities;
using ShoeProductionApp.Services;
using ShoeProductionApp.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<ArticleCodeService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ReferenceDataService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageOrders", policy =>
        policy.RequireRole(AppRoles.Manager, AppRoles.Master));

    options.AddPolicy("CanCancelOrders", policy =>
        policy.RequireRole(AppRoles.Manager));

    options.AddPolicy("CanManageClients", policy =>
        policy.RequireRole(AppRoles.Manager));

    options.AddPolicy("CanManageReferenceData", policy =>
        policy.RequireRole(AppRoles.Manager, AppRoles.Master));

    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireRole(AppRoles.Administrator));
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    foreach (var roleName in AppRoles.All)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(
                new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Не удалось создать роль «{roleName}»: {errors}");
            }
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();

    var hasUsers = await userManager.Users.AnyAsync();

    if (!hasUsers)
    {
        var userName = app.Configuration["InitialAdmin:UserName"];
        var password = app.Configuration["InitialAdmin:Password"];

        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Для создания первого администратора задайте " +
                "InitialAdmin:UserName и InitialAdmin:Password " +
                "через User Secrets.");
        }

        var admin = new ApplicationUser
        {
            UserName = userName,
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(
            admin, password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                createResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"Не удалось создать администратора: {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(
            admin, AppRoles.Administrator);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                roleResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"Не удалось назначить роль администратора: {errors}");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
