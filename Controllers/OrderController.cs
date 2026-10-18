using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeProductionApp.Authorization;
using ShoeProductionApp.Entities;
using ShoeProductionApp.Services;

namespace ShoeProductionApp.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    // GET: /Orders
    // Список доступен всем авторизованным пользователям.
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

    // GET: /Orders/Details/5
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

    // POST: /Orders/AdvanceStatus/5
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

    // POST: /Orders/Cancel/5
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
