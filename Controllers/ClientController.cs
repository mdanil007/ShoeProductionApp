using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeProductionApp.Entities;
using ShoeProductionApp.Services;
using ShoeProductionApp.ViewModels;

namespace ShoeProductionApp.Controllers;

[Authorize(Roles = "Руководитель,Мастер,Менеджер")]
public class ClientsController : Controller
{
    private readonly ClientService _clientService;
    public ClientsController(ClientService clientService) => _clientService = clientService;

    [HttpGet]
    public async Task<IActionResult> Index(string? search, ClientType? type)
    {
        try
        {
            return View(new ClientIndexViewModel
            {
                Search = search,
                Type = type,
                Clients = await _clientService.GetAllAsync(search, type)
            });
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View(new ClientIndexViewModel
            {
                Search = search,
                Clients = await _clientService.GetAllAsync(search)
            });
        }
    }

    [HttpGet, Authorize(Policy = "CanManageClients")]
    public IActionResult Create() => View("Form", new ClientFormViewModel { PageTitle = "Добавить клиента" });

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "CanManageClients")]
    public async Task<IActionResult> Create(ClientFormViewModel model)
    {
        if (!ModelState.IsValid) { model.PageTitle = "Добавить клиента"; return View("Form", model); }
        try
        {
            await _clientService.CreateAsync(model.Name, model.Type, model.ContactPerson, model.Phone, model.Email, model.Address, model.Comment);
            TempData["SuccessMessage"] = "Клиент успешно добавлен.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.PageTitle = "Добавить клиента";
            return View("Form", model);
        }
    }

    [HttpGet, Authorize(Policy = "CanManageClients")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var c = await _clientService.GetByIdAsync(id);
            return View("Form", new ClientFormViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                ContactPerson = c.ContactPerson,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                Comment = c.Comment,
                PageTitle = "Редактировать клиента"
            });
        }
        catch (KeyNotFoundException ex) { TempData["ErrorMessage"] = ex.Message; return RedirectToAction(nameof(Index)); }
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "CanManageClients")]
    public async Task<IActionResult> Edit(int id, ClientFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) { model.PageTitle = "Редактировать клиента"; return View("Form", model); }
        try
        {
            await _clientService.UpdateAsync(id, model.Name, model.Type, model.ContactPerson, model.Phone, model.Email, model.Address, model.Comment);
            TempData["SuccessMessage"] = "Данные клиента обновлены.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException ex) { TempData["ErrorMessage"] = ex.Message; return RedirectToAction(nameof(Index)); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.PageTitle = "Редактировать клиента";
            return View("Form", model);
        }
    }

    [HttpGet, Authorize(Policy = "CanManageClients")]
    public async Task<IActionResult> Delete(int id)
    {
        try { return View(await _clientService.GetByIdAsync(id)); }
        catch (KeyNotFoundException ex) { TempData["ErrorMessage"] = ex.Message; return RedirectToAction(nameof(Index)); }
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Policy = "CanManageClients")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try { await _clientService.DeleteAsync(id); TempData["SuccessMessage"] = "Клиент удалён."; }
        catch (KeyNotFoundException ex) { TempData["ErrorMessage"] = ex.Message; }
        catch (InvalidOperationException ex) { TempData["ErrorMessage"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }
}
