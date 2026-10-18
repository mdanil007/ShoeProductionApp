using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoeProductionApp.Authorization;
using ShoeProductionApp.Data;
using ShoeProductionApp.Entities;
using ShoeProductionApp.Services;
using ShoeProductionApp.ViewModels;

namespace ShoeProductionApp.Controllers;

[Authorize(Roles = "Руководитель,Мастер,Менеджер")]
public class ReferenceController : Controller
{
    private readonly ReferenceDataService _referenceDataService;
    private readonly ProductService _productService;
    private readonly ApplicationDbContext _db;

    public ReferenceController(
        ReferenceDataService referenceDataService,
        ProductService productService,
        ApplicationDbContext db)
    {
        _referenceDataService = referenceDataService;
        _productService = productService;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string tab = "models",
        string? searchModels = null,
        string? searchArticles = null,
        string? searchColors = null,
        string? searchLeathers = null,
        string? searchLinings = null)
    {
        var vm = new ReferenceIndexViewModel
        {
            Tab = NormalizeTab(tab),
            SearchModels = searchModels,
            SearchArticles = searchArticles,
            SearchColors = searchColors,
            SearchLeathers = searchLeathers,
            SearchLinings = searchLinings,
            Models = await _referenceDataService.GetModelsAsync(searchModels),
            Colors = await _referenceDataService.GetColorsAsync(searchColors),
            Leathers = await _referenceDataService.GetLeathersAsync(searchLeathers),
            Linings = await _referenceDataService.GetLiningsAsync(searchLinings),
            Articles = await _productService.GetAllAsync(searchArticles),
            Products = await _db.Products.AsNoTracking().ToListAsync()
        };

        return View(vm);
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateModel(string name, string code, int colorId)
    {
        return await ExecuteAsync("models", async () =>
            await _referenceDataService.CreateModelAsync(name, code, colorId));
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateModel(int id, string name, string code, int colorId)
    {
        return await ExecuteAsync("models", async () =>
            await _referenceDataService.UpdateModelAsync(id, name, code, colorId));
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteModel(int id)
    {
        return await ExecuteAsync("models", async () =>
            await _referenceDataService.DeleteModelAsync(id));
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateColor(string name, string code)
    {
        return await ExecuteAsync("colors", async () =>
            await _referenceDataService.CreateColorAsync(name, code));
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateColor(int id, string name, string code)
    {
        return await ExecuteAsync("colors", async () =>
            await _referenceDataService.UpdateColorAsync(id, name, code));
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteColor(int id)
    {
        return await ExecuteAsync("colors", async () =>
            await _referenceDataService.DeleteColorAsync(id));
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLeather(string name, string code)
    {
        return await ExecuteAsync("leathers", async () =>
            await _referenceDataService.CreateLeatherAsync(name, code));
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLeather(int id, string name, string code)
    {
        return await ExecuteAsync("leathers", async () =>
            await _referenceDataService.UpdateLeatherAsync(id, name, code));
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLeather(int id)
    {
        return await ExecuteAsync("leathers", async () =>
            await _referenceDataService.DeleteLeatherAsync(id));
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLining(string name, string code)
    {
        return await ExecuteAsync("linings", async () =>
            await _referenceDataService.CreateLiningAsync(name, code));
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLining(int id, string name, string code)
    {
        return await ExecuteAsync("linings", async () =>
            await _referenceDataService.UpdateLiningAsync(id, name, code));
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLining(int id)
    {
        return await ExecuteAsync("linings", async () =>
            await _referenceDataService.DeleteLiningAsync(id));
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateArticle(Gender gender, int modelId, int leatherId, int liningId)
    {
        return await ExecuteAsync("articles", async () =>
            await _productService.CreateAsync(gender, modelId, leatherId, liningId));
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateArticle(int id, Gender gender, int modelId, int leatherId, int liningId)
    {
        return await ExecuteAsync("articles", async () =>
            await _productService.UpdateAsync(id, gender, modelId, leatherId, liningId));
    }

    [Authorize(Policy = "CanManageReferenceData")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteArticle(int id)
    {
        return await ExecuteAsync("articles", async () =>
            await _productService.DeleteAsync(id));
    }

    private async Task<IActionResult> ExecuteAsync(string tab, Func<Task> operation)
    {
        try
        {
            await operation();
            TempData["SuccessMessage"] = "Изменения сохранены.";
        }
        catch (Exception ex) when (
            ex is ArgumentException ||
            ex is InvalidOperationException ||
            ex is KeyNotFoundException ||
            ex is DbUpdateException)
        {
            TempData["ErrorMessage"] = ex is DbUpdateException
                ? "Не удалось сохранить изменения. Проверьте, не используется ли эта запись в других данных."
                : ex.Message;
        }

        return RedirectToAction(nameof(Index), new { tab });
    }

    private static string NormalizeTab(string? tab) =>
        tab is "models" or "articles" or "colors" or "leathers" or "linings"
            ? tab
            : "models";
}
