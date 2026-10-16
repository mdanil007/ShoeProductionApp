using Microsoft.EntityFrameworkCore;
using ShoeProductionApp.Data;
using ShoeProductionApp.Entities;

namespace ShoeProductionApp.Services;

public class ReferenceDataService
{
    private readonly ApplicationDbContext _db;

    public ReferenceDataService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Model>> GetModelsAsync(string? search = null)
    {
        var query = _db.Models
            .Include(x => x.Color)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(x.Name, $"%{term}%") ||
                EF.Functions.ILike(x.Code, $"%{term}%"));
        }

        return await query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Code)
            .ToListAsync();
    }

    public async Task<Model> CreateModelAsync(
        string name,
        string code,
        int colorId)
    {
        name = ValidateName(name);
        code = ValidateCode(code, 2, "модели");

        var colorExists = await _db.Colors
            .AnyAsync(x => x.Id == colorId);

        if (!colorExists)
        {
            throw new InvalidOperationException(
                "Выбранный цвет не найден.");
        }

        var duplicate = await _db.Models.AnyAsync(x =>
            x.Code == code && x.ColorId == colorId);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "Модель с таким кодом и цветом уже существует.");
        }

        var model = new Model
        {
            Name = name,
            Code = code,
            ColorId = colorId
        };

        _db.Models.Add(model);
        await _db.SaveChangesAsync();

        return model;
    }

    public async Task<Model> UpdateModelAsync(
        int id,
        string name,
        string code,
        int colorId)
    {
        var model = await _db.Models
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Модель не найдена.");

        if (await _db.Products.AnyAsync(x => x.ModelId == id))
        {
            throw new InvalidOperationException(
                "Нельзя изменять модель, которая используется в артикулах.");
        }

        name = ValidateName(name);
        code = ValidateCode(code, 2, "модели");

        if (!await _db.Colors.AnyAsync(x => x.Id == colorId))
        {
            throw new InvalidOperationException(
                "Выбранный цвет не найден.");
        }

        var duplicate = await _db.Models.AnyAsync(x =>
            x.Id != id &&
            x.Code == code &&
            x.ColorId == colorId);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "Модель с таким кодом и цветом уже существует.");
        }

        model.Name = name;
        model.Code = code;
        model.ColorId = colorId;

        await _db.SaveChangesAsync();
        return model;
    }

    public async Task DeleteModelAsync(int id)
    {
        var model = await _db.Models
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Модель не найдена.");

        if (await _db.Products.AnyAsync(x => x.ModelId == id))
        {
            throw new InvalidOperationException(
                "Нельзя удалить модель, которая используется в артикулах.");
        }

        _db.Models.Remove(model);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Color>> GetColorsAsync(string? search = null)
    {
        var query = _db.Colors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(x.Name, $"%{term}%") ||
                EF.Functions.ILike(x.Code, $"%{term}%"));
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<Color> CreateColorAsync(string name, string code)
    {
        name = ValidateName(name);
        code = ValidateCode(code, 1, "цвета");

        if (await _db.Colors.AnyAsync(x => x.Code == code))
        {
            throw new InvalidOperationException(
                "Цвет с таким кодом уже существует.");
        }

        var color = new Color { Name = name, Code = code };

        _db.Colors.Add(color);
        await _db.SaveChangesAsync();

        return color;
    }

    public async Task<Color> UpdateColorAsync(
        int id,
        string name,
        string code)
    {
        var color = await _db.Colors
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Цвет не найден.");

        if (await _db.Models.AnyAsync(x => x.ColorId == id))
        {
            throw new InvalidOperationException(
                "Нельзя изменять цвет, который используется моделями.");
        }

        name = ValidateName(name);
        code = ValidateCode(code, 1, "цвета");

        if (await _db.Colors.AnyAsync(x =>
                x.Id != id && x.Code == code))
        {
            throw new InvalidOperationException(
                "Цвет с таким кодом уже существует.");
        }

        color.Name = name;
        color.Code = code;

        await _db.SaveChangesAsync();
        return color;
    }

    public async Task DeleteColorAsync(int id)
    {
        var color = await _db.Colors
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Цвет не найден.");

        if (await _db.Models.AnyAsync(x => x.ColorId == id))
        {
            throw new InvalidOperationException(
                "Нельзя удалить цвет, который используется моделями.");
        }

        _db.Colors.Remove(color);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Leather>> GetLeathersAsync(
        string? search = null)
    {
        var query = _db.Leathers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(x.Name, $"%{term}%") ||
                EF.Functions.ILike(x.Code, $"%{term}%"));
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<Leather> CreateLeatherAsync(
        string name,
        string code)
    {
        name = ValidateName(name);
        code = ValidateCode(code, 1, "кожи");

        if (await _db.Leathers.AnyAsync(x => x.Code == code))
        {
            throw new InvalidOperationException(
                "Кожа с таким кодом уже существует.");
        }

        var leather = new Leather { Name = name, Code = code };

        _db.Leathers.Add(leather);
        await _db.SaveChangesAsync();

        return leather;
    }

    public async Task<Leather> UpdateLeatherAsync(
        int id,
        string name,
        string code)
    {
        var leather = await _db.Leathers
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Кожа не найдена.");

        if (await _db.Products.AnyAsync(x => x.LeatherId == id))
        {
            throw new InvalidOperationException(
                "Нельзя изменять кожу, которая используется в артикулах.");
        }

        name = ValidateName(name);
        code = ValidateCode(code, 1, "кожи");

        if (await _db.Leathers.AnyAsync(x =>
                x.Id != id && x.Code == code))
        {
            throw new InvalidOperationException(
                "Кожа с таким кодом уже существует.");
        }

        leather.Name = name;
        leather.Code = code;

        await _db.SaveChangesAsync();
        return leather;
    }

    public async Task DeleteLeatherAsync(int id)
    {
        var leather = await _db.Leathers
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Кожа не найдена.");

        if (await _db.Products.AnyAsync(x => x.LeatherId == id))
        {
            throw new InvalidOperationException(
                "Нельзя удалить кожу, которая используется в артикулах.");
        }

        _db.Leathers.Remove(leather);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Lining>> GetLiningsAsync(
        string? search = null)
    {
        var query = _db.Linings.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(x.Name, $"%{term}%") ||
                EF.Functions.ILike(x.Code, $"%{term}%"));
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<Lining> CreateLiningAsync(
        string name,
        string code)
    {
        name = ValidateName(name);
        code = ValidateCode(code, 1, "подкладки");

        if (await _db.Linings.AnyAsync(x => x.Code == code))
        {
            throw new InvalidOperationException(
                "Подкладка с таким кодом уже существует.");
        }

        var lining = new Lining { Name = name, Code = code };

        _db.Linings.Add(lining);
        await _db.SaveChangesAsync();

        return lining;
    }

    public async Task<Lining> UpdateLiningAsync(
        int id,
        string name,
        string code)
    {
        var lining = await _db.Linings
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Подкладка не найдена.");

        if (await _db.Products.AnyAsync(x => x.LiningId == id))
        {
            throw new InvalidOperationException(
                "Нельзя изменять подкладку, которая используется в артикулах.");
        }

        name = ValidateName(name);
        code = ValidateCode(code, 1, "подкладки");

        if (await _db.Linings.AnyAsync(x =>
                x.Id != id && x.Code == code))
        {
            throw new InvalidOperationException(
                "Подкладка с таким кодом уже существует.");
        }

        lining.Name = name;
        lining.Code = code;

        await _db.SaveChangesAsync();
        return lining;
    }

    public async Task DeleteLiningAsync(int id)
    {
        var lining = await _db.Linings
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Подкладка не найдена.");

        if (await _db.Products.AnyAsync(x => x.LiningId == id))
        {
            throw new InvalidOperationException(
                "Нельзя удалить подкладку, которая используется в артикулах.");
        }

        _db.Linings.Remove(lining);
        await _db.SaveChangesAsync();
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Название обязательно для заполнения.");
        }

        return name.Trim();
    }

    private static string ValidateCode(
        string code,
        int length,
        string entityName)
    {
        code = code?.Trim() ?? string.Empty;

        var isValid = code.Length == length &&
                      code.All(c => c >= '0' && c <= '9');

        if (!isValid)
        {
            var description = length == 1
                ? "одну цифру"
                : "две цифры";

            throw new ArgumentException(
                $"Код {entityName} должен содержать {description}.");
        }

        return code;
    }
}
