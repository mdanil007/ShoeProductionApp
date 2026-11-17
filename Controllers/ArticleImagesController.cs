using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeProductionApp.Data;

namespace ShoeProductionApp.Controllers;

[Authorize(Roles = "Менеджер,Мастер")]
[Route("article-images")]
public class ArticleImagesController(ApplicationDbContext db, IWebHostEnvironment environment) : Controller
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var fileName = await db.Products.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => x.ImageFileName)
            .SingleOrDefaultAsync();
        if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName)) return NotFound();
        var path = Path.Combine(environment.ContentRootPath, "App_Data", "Uploads", "Articles", fileName);
        if (!System.IO.File.Exists(path)) return NotFound();
        var contentType = Path.GetExtension(fileName).ToLowerInvariant() switch
        { ".jpg" => "image/jpeg", ".png" => "image/png", ".webp" => "image/webp", _ => "application/octet-stream" };
        return PhysicalFile(path, contentType);
    }
}
