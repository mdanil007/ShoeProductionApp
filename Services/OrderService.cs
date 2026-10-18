using Microsoft.EntityFrameworkCore;
using ShoeProductionApp.Data;
using ShoeProductionApp.Entities;
using ShoeProductionApp.ViewModel;
using ShoeProductionApp.ViewModels;

namespace ShoeProductionApp.Services;

public class OrderService
{
    private readonly ApplicationDbContext _db;
    private readonly ArticleCodeService _articleCodeService;

    public OrderService(
        ApplicationDbContext db,
        ArticleCodeService articleCodeService)
    {
        _db = db;
        _articleCodeService = articleCodeService;
    }

    // Создание заказа вместе с размерами и начальной историей статусов.
    public async Task<Order> CreateAsync(
        int clientId,
        int productId,
        string createdByUserId,
        DateOnly plannedShipmentDate,
        string? comment,
        IEnumerable<OrderSizeInputViewModel> sizes)
    {
        if (string.IsNullOrWhiteSpace(createdByUserId))
        {
            throw new ArgumentException(
                "Не удалось определить пользователя, создающего заказ.");
        }

        var todayInMoscow = DateOnly.FromDateTime(
            DateTime.UtcNow.AddHours(3).Date);

        if (plannedShipmentDate < todayInMoscow)
        {
            throw new ArgumentException(
                "Плановая дата отправки не может быть в прошлом.");
        }

        if (!await _db.Clients.AnyAsync(x => x.Id == clientId))
        {
            throw new InvalidOperationException("Клиент не найден.");
        }

        var product = await _db.Products
            .Include(x => x.Model)
                .ThenInclude(x => x.Color)
            .Include(x => x.Leather)
            .Include(x => x.Lining)
            .SingleOrDefaultAsync(x => x.Id == productId);

        if (product is null)
        {
            throw new InvalidOperationException("Артикул не найден.");
        }

        // Проверяем, что артикул действительно можно сформировать.
        _articleCodeService.BuildArticleCode(product);

        if (!await _db.Users.AnyAsync(x =>
                x.Id == createdByUserId && x.IsActive))
        {
            throw new InvalidOperationException(
                "Пользователь не найден или его учётная запись заблокирована.");
        }

        var sizeList = ValidateSizes(sizes);

        var order = new Order
        {
            ClientId = clientId,
            ProductId = productId,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTimeOffset.UtcNow,
            PlannedShipmentDate = plannedShipmentDate,
            Comment = string.IsNullOrWhiteSpace(comment)
                ? null
                : comment.Trim(),
            Status = OrderStatus.Created
        };

        foreach (var size in sizeList)
        {
            order.Sizes.Add(new OrderSize
            {
                Size = size.Size,
                Count = size.Count
            });
        }

        order.StatusHistory.Add(new OrderStatusHistory
        {
            Status = OrderStatus.Created,
            ChangedAt = DateTimeOffset.UtcNow,
            ChangedByUserId = createdByUserId
        });

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return order;
    }

    // Список заказов с поиском и фильтрами.
    public async Task<List<OrderListItemViewModel>> GetAllAsync(
        string? search = null,
        OrderStatus? status = null,
        DateOnly? plannedShipmentDate = null,
        DateOnly? actualShipmentDate = null)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value))
        {
            throw new ArgumentException("Недопустимый статус заказа.");
        }

        var orders = await _db.Orders
            .AsNoTracking()
            .Include(x => x.Client)
            .Include(x => x.Product)
                .ThenInclude(x => x.Model)
                    .ThenInclude(x => x.Color)
            .Include(x => x.Product.Leather)
            .Include(x => x.Product.Lining)
            .Include(x => x.Sizes)
            .ToListAsync();

        var todayInMoscow = DateOnly.FromDateTime(
            DateTime.UtcNow.AddHours(3).Date);

        var query = orders.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(x =>
                x.Client.Name.Contains(
                    term, StringComparison.OrdinalIgnoreCase) ||
                _articleCodeService.BuildArticleCode(x.Product)
                    .Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (plannedShipmentDate.HasValue)
        {
            query = query.Where(
                x => x.PlannedShipmentDate == plannedShipmentDate.Value);
        }

        if (actualShipmentDate.HasValue)
        {
            query = query.Where(x =>
                x.ActualShipmentDate.HasValue &&
                DateOnly.FromDateTime(
                    x.ActualShipmentDate.Value.UtcDateTime.AddHours(3)) ==
                actualShipmentDate.Value);
        }

        return query
            .Select(x => new OrderListItemViewModel
            {
                Id = x.Id,
                ClientName = x.Client.Name,
                Article = _articleCodeService.BuildArticleCode(x.Product),
                TotalPairs = x.Sizes.Sum(s => s.Count),
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                PlannedShipmentDate = x.PlannedShipmentDate,
                ActualShipmentDate = x.ActualShipmentDate,
                IsOverdue =
                    x.PlannedShipmentDate < todayInMoscow &&
                    x.Status != OrderStatus.Shipped &&
                    x.Status != OrderStatus.Cancelled
            })
            .OrderBy(x =>
                x.Status == OrderStatus.Shipped ||
                x.Status == OrderStatus.Cancelled)
            .ThenBy(x => x.CreatedAt)
            .ToList();
    }

    // Подробности заказа, размеры и история статусов.
    public async Task<OrderDetailsViewModel> GetByIdAsync(int id)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(x => x.Client)
            .Include(x => x.CreatedByUser)
            .Include(x => x.Product)
                .ThenInclude(x => x.Model)
                    .ThenInclude(x => x.Color)
            .Include(x => x.Product.Leather)
            .Include(x => x.Product.Lining)
            .Include(x => x.Sizes)
            .Include(x => x.StatusHistory)
                .ThenInclude(x => x.ChangedByUser)
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Заказ не найден.");

        return new OrderDetailsViewModel
        {
            Id = order.Id,
            ClientName = order.Client.Name,
            Article = _articleCodeService.BuildArticleCode(order.Product),
            CreatorName = order.CreatedByUser.UserName ?? "Неизвестно",
            CreatedAt = order.CreatedAt,
            PlannedShipmentDate = order.PlannedShipmentDate,
            ActualShipmentDate = order.ActualShipmentDate,
            Comment = order.Comment,
            Status = order.Status,
            TotalPairs = order.Sizes.Sum(x => x.Count),

            Sizes = order.Sizes
                .OrderBy(x => int.Parse(x.Size))
                .Select(x => new OrderSizeInputViewModel
                {
                    Size = x.Size,
                    Count = x.Count
                })
                .ToList(),

            StatusHistory = order.StatusHistory
                .OrderBy(x => x.ChangedAt)
                .Select(x => new OrderStatusHistoryItemViewModel
                {
                    Status = x.Status,
                    ChangedAt = x.ChangedAt,
                    ChangedByUserName =
                        x.ChangedByUser.UserName ?? "Неизвестно"
                })
                .ToList()
        };
    }


    // Перевод заказа на следующий производственный этап.
    public async Task AdvanceStatusAsync(
        int orderId,
        string changedByUserId)
    {
        ValidateUserId(changedByUserId);

        var userIsActive = await _db.Users.AnyAsync(x =>
            x.Id == changedByUserId && x.IsActive);

        if (!userIsActive)
        {
            throw new InvalidOperationException(
                "Пользователь не найден или его учётная запись заблокирована.");
        }

        var order = await _db.Orders
            .SingleOrDefaultAsync(x => x.Id == orderId)
            ?? throw new KeyNotFoundException("Заказ не найден.");

        if (order.Status == OrderStatus.Shipped)
        {
            throw new InvalidOperationException(
                "Заказ уже отправлен. Изменение статуса невозможно.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Отменённый заказ нельзя продолжить.");
        }

        if (!Enum.IsDefined(order.Status))
        {
            throw new InvalidOperationException(
                "У заказа обнаружен неизвестный статус.");
        }

        var nextStatus = (OrderStatus)((int)order.Status + 1);
        var now = DateTimeOffset.UtcNow;

        order.Status = nextStatus;

        if (nextStatus == OrderStatus.Shipped)
        {
            order.ActualShipmentDate = now;
        }

        _db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = nextStatus,
            ChangedAt = now,
            ChangedByUserId = changedByUserId
        });

        // Изменение заказа и запись истории сохраняются вместе.
        await _db.SaveChangesAsync();
    }

    // Отмена заказа. Право отмены руководителем проверяется
    // на уровне авторизации контроллера.
    public async Task CancelAsync(
        int orderId,
        string changedByUserId)
    {
        ValidateUserId(changedByUserId);

        var userIsActive = await _db.Users.AnyAsync(x =>
            x.Id == changedByUserId && x.IsActive);

        if (!userIsActive)
        {
            throw new InvalidOperationException(
                "Пользователь не найден или его учётная запись заблокирована.");
        }

        var order = await _db.Orders
            .SingleOrDefaultAsync(x => x.Id == orderId)
            ?? throw new KeyNotFoundException("Заказ не найден.");

        if (order.Status == OrderStatus.Shipped)
        {
            throw new InvalidOperationException(
                "Отправленный заказ отменить нельзя.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Заказ уже отменён.");
        }

        if (!Enum.IsDefined(order.Status) ||
            (int)order.Status > (int)OrderStatus.Shipping)
        {
            throw new InvalidOperationException(
                "Отмена заказа на этом этапе невозможна.");
        }

        var now = DateTimeOffset.UtcNow;

        order.Status = OrderStatus.Cancelled;

        _db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatus.Cancelled,
            ChangedAt = now,
            ChangedByUserId = changedByUserId
        });

        await _db.SaveChangesAsync();
    }

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "Не удалось определить пользователя.");
        }
    }


    private static List<OrderSizeInputViewModel> ValidateSizes(
        IEnumerable<OrderSizeInputViewModel> sizes)
    {
        ArgumentNullException.ThrowIfNull(sizes);

        var list = sizes.ToList();

        if (list.Count == 0)
        {
            throw new ArgumentException(
                "Добавьте хотя бы один размер.");
        }

        var normalized = new List<OrderSizeInputViewModel>();

        foreach (var item in list)
        {
            if (item is null ||
                !int.TryParse(item.Size, out var sizeValue) ||
                sizeValue < 30 ||
                sizeValue > 60 ||
                item.Size != sizeValue.ToString())
            {
                throw new ArgumentException(
                    "Размер должен быть целым числом от 30 до 60.");
            }

            if (item.Count <= 0)
            {
                throw new ArgumentException(
                    "Количество пар для каждого размера должно быть больше нуля.");
            }

            normalized.Add(new OrderSizeInputViewModel
            {
                Size = item.Size,
                Count = item.Count
            });
        }

        if (normalized
            .GroupBy(x => x.Size)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Один и тот же размер нельзя добавлять несколько раз.");
        }

        return normalized;
    }
}
