using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeProductionApp.Authorization;
using ShoeProductionApp.Data;
using ShoeProductionApp.Entities;
using ShoeProductionApp.Services;
using ShoeProductionApp.ViewModels;

namespace ShoeProductionApp.Controllers;

[Authorize(Roles = "Менеджер,Мастер")]
public class ReferenceController(ReferenceDataService references, ProductService products, ApplicationDbContext db) : Controller
{
    private readonly ReferenceDataService _references=references;
    private readonly ProductService _products=products;
    private readonly ApplicationDbContext _db=db;

    [HttpGet]
    public async Task<IActionResult> Index(string tab="articles",string? searchModels=null,string? searchArticles=null,string? searchColors=null,string? searchLeathers=null,string? searchLinings=null,string? searchSoles=null)
    {
        var vm=new ReferenceIndexViewModel { Tab=NormalizeTab(tab), SearchModels=searchModels,SearchArticles=searchArticles,SearchColors=searchColors,SearchLeathers=searchLeathers,SearchLinings=searchLinings,SearchSoles=searchSoles, Models=await _references.GetModelsAsync(searchModels),Colors=await _references.GetColorsAsync(searchColors),Leathers=await _references.GetLeathersAsync(searchLeathers),Linings=await _references.GetLiningsAsync(searchLinings),Soles=await _references.GetSolesAsync(searchSoles),Articles=await _products.GetAllAsync(searchArticles),Products=await _db.Products.AsNoTracking().ToListAsync() };
        return View(vm);
    }

    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> CreateModel(string name,string code)=>Run("models",()=>_references.CreateModelAsync(name,code));
    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> UpdateModel(int id,string name,string code)=>Run("models",()=>_references.UpdateModelAsync(id,name,code));
    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteModel(int id)=>Run("models",()=>_references.DeleteModelAsync(id));

    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> CreateColor(string name,string code)=>Run("colors",()=>_references.CreateColorAsync(name,code));
    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> UpdateColor(int id,string name,string code)=>Run("colors",()=>_references.UpdateColorAsync(id,name,code));
    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteColor(int id)=>Run("colors",()=>_references.DeleteColorAsync(id));

    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> CreateLeather(string name,string code)=>Run("leathers",()=>_references.CreateLeatherAsync(name,code));
    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> UpdateLeather(int id,string name,string code)=>Run("leathers",()=>_references.UpdateLeatherAsync(id,name,code));
    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteLeather(int id)=>Run("leathers",()=>_references.DeleteLeatherAsync(id));

    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> CreateLining(string name,string code)=>Run("linings",()=>_references.CreateLiningAsync(name,code));
    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> UpdateLining(int id,string name,string code)=>Run("linings",()=>_references.UpdateLiningAsync(id,name,code));
    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteLining(int id)=>Run("linings",()=>_references.DeleteLiningAsync(id));

    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> CreateSole(string name,string code)=>Run("soles",()=>_references.CreateSoleAsync(name,code));
    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> UpdateSole(int id,string name,string code)=>Run("soles",()=>_references.UpdateSoleAsync(id,name,code));
    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteSole(int id)=>Run("soles",()=>_references.DeleteSoleAsync(id));

    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> CreateArticle(Gender gender,int modelId,int colorId,int leatherId,int liningId,int soleId,IFormFile? image)=>Run("articles",()=>_products.CreateAsync(gender,modelId,colorId,leatherId,liningId,soleId,image));
    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> UpdateArticle(int id,Gender gender,int modelId,int colorId,int leatherId,int liningId,int soleId,IFormFile? image,bool removeImage=false)=>Run("articles",()=>_products.UpdateAsync(id,gender,modelId,colorId,leatherId,liningId,soleId,image,removeImage));
    [Authorize(Policy="CanManageReferenceData"),HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteArticle(int id)=>Run("articles",()=>_products.DeleteAsync(id));

    private async Task<IActionResult> Run(string tab,Func<Task> action)
    {
        try { await action(); TempData["SuccessMessage"]="Изменения сохранены."; }
        catch(Exception ex) when(ex is ArgumentException or InvalidOperationException or KeyNotFoundException or DbUpdateException) { TempData["ErrorMessage"]=ex is DbUpdateException?"Не удалось сохранить изменения. Проверьте уникальность кода и использование записи.":ex.Message; }
        return RedirectToAction(nameof(Index),new {tab});
    }
    private static string NormalizeTab(string? tab)=>tab is "articles" or "models" or "colors" or "leathers" or "linings" or "soles"?tab:"articles";
}
