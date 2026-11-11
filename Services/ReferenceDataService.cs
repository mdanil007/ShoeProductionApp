using Microsoft.EntityFrameworkCore;
using ShoeProductionApp.Data;
using ShoeProductionApp.Entities;

namespace ShoeProductionApp.Services;

public class ReferenceDataService(ApplicationDbContext db)
{
    private readonly ApplicationDbContext _db = db;

    public Task<List<Model>> GetModelsAsync(string? search = null) => Search(_db.Models, search);
    public Task<List<Color>> GetColorsAsync(string? search = null) => Search(_db.Colors, search);
    public Task<List<Leather>> GetLeathersAsync(string? search = null) => Search(_db.Leathers, search);
    public Task<List<Lining>> GetLiningsAsync(string? search = null) => Search(_db.Linings, search);
    public Task<List<Sole>> GetSolesAsync(string? search = null) => Search(_db.Soles, search);

    private async Task<List<T>> Search<T>(IQueryable<T> source, string? search) where T : class
    {
        var query = source.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(EF.Property<string>(x, "Name"), $"%{term}%") ||
                EF.Functions.ILike(EF.Property<string>(x, "Code"), $"%{term}%"));
        }
        return await query.OrderBy(x => EF.Property<string>(x, "Name")).ToListAsync();
    }

    public Task<Model> CreateModelAsync(string name, string code) => CreateNamed(_db.Models, name, code, 3, "модели");
    public Task<Color> CreateColorAsync(string name, string code) => CreateNamed(_db.Colors, name, code, 2, "цвета");
    public Task<Leather> CreateLeatherAsync(string name, string code) => CreateNamed(_db.Leathers, name, code, 1, "кожи");
    public Task<Lining> CreateLiningAsync(string name, string code) => CreateNamed(_db.Linings, name, code, 1, "подкладки");
    public Task<Sole> CreateSoleAsync(string name, string code) => CreateNamed(_db.Soles, name, code, 3, "подошвы");

    private async Task<T> CreateNamed<T>(DbSet<T> set, string name, string code, int length, string label) where T : class, new()
    {
        name = ValidateName(name); code = ValidateCode(code, length, label);
        var prop = typeof(T).GetProperty(nameof(Model.Code))!;
        if (await set.AnyAsync(x => EF.Property<string>(x, "Code") == code)) throw new InvalidOperationException($"Запись справочника «{label}» с таким кодом уже существует.");
        var item = new T(); typeof(T).GetProperty(nameof(Model.Name))!.SetValue(item, name); prop.SetValue(item, code);
        set.Add(item); await _db.SaveChangesAsync(); return item;
    }

    public Task UpdateModelAsync(int id,string name,string code) => UpdateNamed(_db.Models,id,name,code,3,"модели", x=>_db.Products.AnyAsync(p=>p.ModelId==id));
    public Task UpdateColorAsync(int id,string name,string code) => UpdateNamed(_db.Colors,id,name,code,2,"цвета", x=>_db.Products.AnyAsync(p=>p.ColorId==id));
    public Task UpdateLeatherAsync(int id,string name,string code) => UpdateNamed(_db.Leathers,id,name,code,1,"кожи", x=>_db.Products.AnyAsync(p=>p.LeatherId==id));
    public Task UpdateLiningAsync(int id,string name,string code) => UpdateNamed(_db.Linings,id,name,code,1,"подкладки", x=>_db.Products.AnyAsync(p=>p.LiningId==id));
    public Task UpdateSoleAsync(int id,string name,string code) => UpdateNamed(_db.Soles,id,name,code,3,"подошвы", x=>_db.Products.AnyAsync(p=>p.SoleId==id));

    private async Task UpdateNamed<T>(DbSet<T> set,int id,string name,string code,int length,string label,Func<int,Task<bool>> inUse) where T:class
    {
        var item=await set.FindAsync(id) ?? throw new KeyNotFoundException("Запись справочника не найдена.");
        if(await inUse(id)) throw new InvalidOperationException("Нельзя изменять составляющую, которая используется в артикулах.");
        name=ValidateName(name); code=ValidateCode(code,length,label);
        if(await set.AnyAsync(x=>EF.Property<string>(x,"Code")==code && EF.Property<int>(x,"Id")!=id)) throw new InvalidOperationException("Запись с таким кодом уже существует.");
        typeof(T).GetProperty("Name")!.SetValue(item,name); typeof(T).GetProperty("Code")!.SetValue(item,code); await _db.SaveChangesAsync();
    }

    public Task DeleteModelAsync(int id)=>DeleteNamed(_db.Models,id,()=>_db.Products.AnyAsync(x=>x.ModelId==id));
    public Task DeleteColorAsync(int id)=>DeleteNamed(_db.Colors,id,()=>_db.Products.AnyAsync(x=>x.ColorId==id));
    public Task DeleteLeatherAsync(int id)=>DeleteNamed(_db.Leathers,id,()=>_db.Products.AnyAsync(x=>x.LeatherId==id));
    public Task DeleteLiningAsync(int id)=>DeleteNamed(_db.Linings,id,()=>_db.Products.AnyAsync(x=>x.LiningId==id));
    public Task DeleteSoleAsync(int id)=>DeleteNamed(_db.Soles,id,()=>_db.Products.AnyAsync(x=>x.SoleId==id));

    private async Task DeleteNamed<T>(DbSet<T> set,int id,Func<Task<bool>> inUse) where T:class
    { var item=await set.FindAsync(id)??throw new KeyNotFoundException("Запись справочника не найдена."); if(await inUse()) throw new InvalidOperationException("Нельзя удалить составляющую, которая используется в артикулах."); set.Remove(item); await _db.SaveChangesAsync(); }

    private static string ValidateName(string name) { if(string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Название обязательно для заполнения."); return name.Trim(); }
    private static string ValidateCode(string code,int length,string label) { code=code?.Trim()??""; if(code.Length!=length || code.Any(c=>c<'0'||c>'9')) throw new ArgumentException($"Код {label} должен содержать {length} цифр."); return code; }
}
