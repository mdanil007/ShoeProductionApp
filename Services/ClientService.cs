using Microsoft.EntityFrameworkCore;
using ShoeProductionApp.Data;
using ShoeProductionApp.Entities;

namespace ShoeProductionApp.Services;

public class ClientService
{
    private readonly ApplicationDbContext _db;

    public ClientService(ApplicationDbContext db)
    {
        _db = db;
    }

    // Получение списка клиентов с поиском и фильтрацией по типу.
    public async Task<List<Client>> GetAllAsync(
        string? search = null,
        ClientType? type = null)
    {
        var query = _db.Clients
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(x.Name, $"%{term}%") ||
                (x.ContactPerson != null &&
                 EF.Functions.ILike(x.ContactPerson, $"%{term}%")) ||
                (x.Phone != null &&
                 EF.Functions.ILike(x.Phone, $"%{term}%")) ||
                (x.Email != null &&
                 EF.Functions.ILike(x.Email, $"%{term}%")) ||
                (x.Address != null &&
                 EF.Functions.ILike(x.Address, $"%{term}%")));
        }

        if (type.HasValue)
        {
            if (!Enum.IsDefined(type.Value))
            {
                throw new ArgumentException(
                    "Указан недопустимый тип клиента.");
            }

            query = query.Where(x => x.Type == type.Value);
        }

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    // Получение одного клиента.
    public async Task<Client> GetByIdAsync(int id)
    {
        return await _db.Clients
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Клиент не найден.");
    }

    // Создание клиента.
    public async Task<Client> CreateAsync(
        string name,
        ClientType type,
        string? contactPerson,
        string? phone,
        string? email,
        string? address,
        string? comment)
    {
        name = ValidateName(name);
        ValidateType(type);

        var client = new Client
        {
            Name = name,
            Type = type,
            ContactPerson = NormalizeOptional(contactPerson),
            Phone = NormalizeOptional(phone),
            Email = NormalizeOptional(email),
            Address = NormalizeOptional(address),
            Comment = NormalizeOptional(comment)
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();

        return client;
    }

    // Изменение клиента.
    public async Task<Client> UpdateAsync(
        int id,
        string name,
        ClientType type,
        string? contactPerson,
        string? phone,
        string? email,
        string? address,
        string? comment)
    {
        var client = await _db.Clients
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Клиент не найден.");

        if (await _db.Orders.AnyAsync(x => x.ClientId == id))
        {
            throw new InvalidOperationException(
                "Нельзя изменять клиента, на которого уже оформлен заказ.");
        }

        name = ValidateName(name);
        ValidateType(type);

        client.Name = name;
        client.Type = type;
        client.ContactPerson = NormalizeOptional(contactPerson);
        client.Phone = NormalizeOptional(phone);
        client.Email = NormalizeOptional(email);
        client.Address = NormalizeOptional(address);
        client.Comment = NormalizeOptional(comment);

        await _db.SaveChangesAsync();

        return client;
    }

    // Удаление клиента.
    public async Task DeleteAsync(int id)
    {
        var client = await _db.Clients
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Клиент не найден.");

        if (await _db.Orders.AnyAsync(x => x.ClientId == id))
        {
            throw new InvalidOperationException(
                "Нельзя удалить клиента, на которого уже оформлен заказ.");
        }

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync();
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Название клиента обязательно для заполнения.");
        }

        return name.Trim();
    }

    private static void ValidateType(ClientType type)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentException(
                "Указан недопустимый тип клиента.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
