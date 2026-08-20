using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Models;
using SistemaGestion.Services.Interfaces;

namespace SistemaGestion.Controllers;

[Authorize]
public class ClientesController : Controller
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    public async Task<IActionResult> Index()
    {
        var clientes = await _clienteService.ListarAsync();
        return View(clientes);
    }

    public IActionResult Crear() => View(new Cliente());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Cliente cliente)
    {
        if (!ModelState.IsValid)
            return View(cliente);

        await _clienteService.CrearAsync(cliente);
        TempData["Mensaje"] = "Cliente creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var cliente = await _clienteService.ObtenerPorIdAsync(id);
        if (cliente is null) return NotFound();
        return View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Cliente cliente)
    {
        if (!ModelState.IsValid)
            return View(cliente);

        await _clienteService.ActualizarAsync(cliente);
        TempData["Mensaje"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _clienteService.EliminarAsync(id);
        TempData["Mensaje"] = "Cliente eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
