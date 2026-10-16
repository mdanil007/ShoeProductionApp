using Microsoft.EntityFrameworkCore;
using ShoeProductionApp.Data;
using ShoeProductionApp.Entities;
using ShoeProductionApp.ViewModels;

namespace ShoeProductionApp.Services;

public class ProductService
{
    private readonly ApplicationDbContext _db;
    private readonly ArticleCodeService _articleCodeService;

    public ProductService(
        ApplicationDbContext db,
        ArticleCodeService articleCodeService)
    {
        _db = db;
        _articleCodeService = articleCodeService;
    }

    public async Task<List<ArticleListItemViewModel>> GetAllAsync()
    {
        var products = await _db.Products
            .Include(x => x.Model)
                .ThenInclude(x => x.Color)
            .Include(x => x.Leather)
            .Include(x => x.Lining)
            .AsNoTracking()
            .ToListAsync();

        return products
            .Select(x => new ArticleListItemViewModel
            {
                Id = x.Id,
                Article = _articleCodeService.BuildArticleCode(x),
                Gender = x.Gender,
                LeatherName = x.Leather.Name,
                LiningName = x.Lining.Name,
                ModelName = x.Model.Name,
                ModelCode = x.Model.Code,
                ColorName = x.Model.Color.Name,
                ColorCode = x.Model.Color.Code
            })
            .OrderBy(x => x.Article)
            .ToList();
    }

    public async Task<Product> CreateAsync(
        Gender gender,
        int modelId,
        int leatherId,
        int liningId)
    {
        if (!Enum.IsDefined(gender))
        {
            throw new ArgumentException("Недопустимый код пола.");
        }

        var model = await _db.Models
            .Include(x => x.Color)
            .SingleOrDefaultAsync(x => x.Id == modelId);

        var leather = await _db.Leathers
            .SingleOrDefaultAsync(x => x.Id == leatherId);

        var lining = await _db.Linings
            .SingleOrDefaultAsync(x => x.Id == liningId);

        if (model is null || leather is null || lining is null)
        {
            throw new InvalidOperationException(
                "Одна или несколько выбранных составляющих не найдены.");
        }

        var alreadyExists = await _db.Products.AnyAsync(x =>
            x.Gender == gender &&
            x.ModelId == modelId &&
            x.LeatherId == leatherId &&
            x.LiningId == liningId);

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "Артикул с такими составляющими уже существует.");
        }

        var product = new Product
        {
            Gender = gender,
            ModelId = modelId,
            Model = model,
            LeatherId = leatherId,
            Leather = leather,
            LiningId = liningId,
            Lining = lining
        };

        // Проверяем, что код формируется корректно.
        _articleCodeService.BuildArticleCode(product);

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return product;
    }
}
