using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ShoeProductionApp.Data;
using ShoeProductionApp.Entities;
using ShoeProductionApp.ViewModels;

namespace ShoeProductionApp.Services;

public class ProductService(
    ApplicationDbContext db,
    ArticleCodeService articleCodeService,
    ArticleImageStorageService imageStorage)
{
    private readonly ApplicationDbContext _db = db;
    private readonly ArticleCodeService _articleCodeService = articleCodeService;
    private readonly ArticleImageStorageService _imageStorage = imageStorage;

    private IQueryable<Product> FullQuery() => _db.Products
        .Include(x => x.Model).Include(x => x.Color).Include(x => x.Leather)
        .Include(x => x.Lining).Include(x => x.Sole);

    public async Task<List<ArticleListItemViewModel>> GetAllAsync(string? search = null)
    {
        var products = await FullQuery().AsNoTracking().ToListAsync();
        var result = products.Select(x => new ArticleListItemViewModel
        {
            Id = x.Id, Article = _articleCodeService.BuildArticleCode(x),
            Gender = x.Gender, ModelId = x.ModelId, ModelName = x.Model.Name, ModelCode = x.Model.Code,
            ColorId = x.ColorId, ColorName = x.Color.Name, ColorCode = x.Color.Code,
            LeatherId = x.LeatherId, LeatherName = x.Leather.Name, LeatherCode = x.Leather.Code,
            LiningId = x.LiningId, LiningName = x.Lining.Name, LiningCode = x.Lining.Code,
            SoleId = x.SoleId, SoleName = x.Sole.Name, SoleCode = x.Sole.Code,
            ImageFileName = x.ImageFileName
        }).OrderBy(x => x.Article).ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            result = result.Where(x => x.Article.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        return result;
    }

    public async Task<Product> CreateAsync(Gender gender, int modelId, int colorId, int leatherId, int liningId, int soleId, IFormFile? image)
    {
        var product = await LoadComponentsAsync(gender, modelId, colorId, leatherId, liningId, soleId);
        await EnsureUniqueAsync(product, null);
        _articleCodeService.BuildArticleCode(product);
        if (image is { Length: > 0 }) product.ImageFileName = await _imageStorage.SaveAsync(image);
        try
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            return product;
        }
        catch
        {
            _imageStorage.Delete(product.ImageFileName);
            throw;
        }
    }

    public async Task<Product> UpdateAsync(int id, Gender gender, int modelId, int colorId, int leatherId, int liningId, int soleId, IFormFile? image, bool removeImage)
    {
        var product = await _db.Products.FindAsync(id) ?? throw new KeyNotFoundException("Артикул не найден.");
        var components = await LoadComponentsAsync(gender, modelId, colorId, leatherId, liningId, soleId);
        var hasOrders = await _db.Orders.AnyAsync(x => x.ProductId == id);
        var componentsChanged = product.Gender != gender || product.ModelId != modelId ||
            product.ColorId != colorId || product.LeatherId != leatherId ||
            product.LiningId != liningId || product.SoleId != soleId;
        if (hasOrders && componentsChanged)
            throw new InvalidOperationException("Нельзя изменять составляющие артикула, который используется в производственных заказах. Фотографию при этом менять можно.");
        await EnsureUniqueAsync(components, id);
        var oldImage = product.ImageFileName;
        string? newImage = null;
        if (image is { Length: > 0 }) newImage = await _imageStorage.SaveAsync(image);

        product.Gender = components.Gender; product.ModelId = components.ModelId; product.Model = components.Model;
        product.ColorId = components.ColorId; product.Color = components.Color;
        product.LeatherId = components.LeatherId; product.Leather = components.Leather;
        product.LiningId = components.LiningId; product.Lining = components.Lining;
        product.SoleId = components.SoleId; product.Sole = components.Sole;
        if (newImage is not null) product.ImageFileName = newImage;
        else if (removeImage) product.ImageFileName = null;

        try
        {
            _articleCodeService.BuildArticleCode(product);
            await _db.SaveChangesAsync();
        }
        catch
        {
            _imageStorage.Delete(newImage);
            throw;
        }
        if (newImage is not null || removeImage) _imageStorage.Delete(oldImage);
        return product;
    }

    private async Task<Product> LoadComponentsAsync(Gender gender, int modelId, int colorId, int leatherId, int liningId, int soleId)
    {
        if (!Enum.IsDefined(gender)) throw new ArgumentException("Недопустимый тип обуви.");
        var model = await _db.Models.FindAsync(modelId); var color = await _db.Colors.FindAsync(colorId);
        var leather = await _db.Leathers.FindAsync(leatherId); var lining = await _db.Linings.FindAsync(liningId);
        var sole = await _db.Soles.FindAsync(soleId);
        if (model is null || color is null || leather is null || lining is null || sole is null)
            throw new InvalidOperationException("Одна или несколько выбранных составляющих не найдены.");
        return new Product { Gender = gender, ModelId = modelId, Model = model, ColorId = colorId, Color = color,
            LeatherId = leatherId, Leather = leather, LiningId = liningId, Lining = lining, SoleId = soleId, Sole = sole };
    }

    private async Task EnsureUniqueAsync(Product p, int? excludeId)
    {
        var duplicate = await _db.Products.AnyAsync(x => (excludeId == null || x.Id != excludeId) && x.Gender == p.Gender &&
            x.ModelId == p.ModelId && x.ColorId == p.ColorId && x.LeatherId == p.LeatherId && x.LiningId == p.LiningId && x.SoleId == p.SoleId);
        if (duplicate) throw new InvalidOperationException("Артикул с такими составляющими уже существует.");
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id) ?? throw new KeyNotFoundException("Артикул не найден.");
        if (await _db.Orders.AnyAsync(x => x.ProductId == id))
            throw new InvalidOperationException("Нельзя удалить артикул, который используется в производственных заказах.");
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        _imageStorage.Delete(product.ImageFileName);
    }
}
