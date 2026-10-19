using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShoeProductionApp.Authorization;
using ShoeProductionApp.Entities;
using ShoeProductionApp.Services;
using ShoeProductionApp.ViewModels;

namespace ShoeProductionApp.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly OrderService _orderService;
    private readonly ClientService _clientService;
    private readonly ProductService _productService;

    public OrdersController(
        OrderService orderService,
        ClientService clientService,
        ProductService productService)
    {
        _orderService = orderService;
        _clientService = clientService;
        _productService = productService;
    }

    private async Task PopulateCreateOrderListsAsync(
    CreateOrderViewModel model)
    {
        var clients = await _clientService.GetAllAsync(null, null);
        var products = await _productService.GetAllAsync();

        model.Clients = clients
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"{x.Name} ({(x.Type == ClientType.Shop ? "Магазин" : "Маркетплейс")})"
            })
            .ToList();

        model.Products = products
            .OrderBy(x => x.Article)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Article
            })
            .ToList();
    }

    public async Task<IActionResult> Index(
        string? search,
        OrderStatus? status,
        DateOnly? plannedShipmentDate,
        DateOnly? actualShipmentDate)
    {
        var orders = await _orderService.GetAllAsync(
            search,
            status,
            plannedShipmentDate,
            actualShipmentDate);

        ViewData["Search"] = search;
        ViewData["Status"] = status;
        ViewData["PlannedShipmentDate"] = plannedShipmentDate;
        ViewData["ActualShipmentDate"] = actualShipmentDate;

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var order = await _orderService.GetByIdAsync(id);
            return View(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    [Authorize(Policy = "CanManageOrders")]
    public async Task<IActionResult> Create()
    {
        var model = new CreateOrderViewModel();

        await PopulateCreateOrderListsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanManageOrders")]
    public async Task<IActionResult> Create(CreateOrderViewModel model)
    {
        model.Sizes = (model.Sizes ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Size))
            .ToList();

        if (model.Sizes.Count == 0)
        {
            ModelState.AddModelError(
                nameof(model.Sizes),
                "Добавьте хотя бы один размер.");
        }

        if (model.PlannedShipmentDate.HasValue)
        {
            var todayInMoscow = DateOnly.FromDateTime(
                DateTime.UtcNow.AddHours(3));

            if (model.PlannedShipmentDate.Value < todayInMoscow)
            {
                ModelState.AddModelError(
                    nameof(model.PlannedShipmentDate),
                    "Плановая дата отгрузки не может быть в прошлом.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateCreateOrderListsAsync(model);
            return View(model);
        }

        var userId = User.FindFirstValue(
            System.Security.Claims.ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Forbid();
        }

        try
        {
            var order = await _orderService.CreateAsync(
                model.ClientId!.Value,
                model.ProductId!.Value,
                userId,
                model.PlannedShipmentDate!.Value,
                model.Comment,
                model.Sizes);

            TempData["SuccessMessage"] =
                $"Заказ №{order.Id} успешно создан.";

            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Выбранный клиент, артикул или пользователь не найден.");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await PopulateCreateOrderListsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanManageOrders")]
    public async Task<IActionResult> AdvanceStatus(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Forbid();
        }

        try
        {
            await _orderService.AdvanceStatusAsync(id, userId);
            TempData["SuccessMessage"] =
                $"Статус заказа №{id} успешно изменён.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanCancelOrders")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Forbid();
        }

        try
        {
            await _orderService.CancelAsync(id, userId);
            TempData["SuccessMessage"] =
                $"Заказ №{id} отменён.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
