using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Models;
using SistemaGestion.Services.Interfaces;

namespace SistemaGestion.Controllers;

[Authorize]
public class ProductosController : Controller
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    public async Task<IActionResult> Index()
    {
        var productos = await _productoService.ListarAsync();
        return View(productos);
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Crear() => View(new Producto());

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Producto producto)
    {
        if (!ModelState.IsValid)
            return View(producto);

        await _productoService.CrearAsync(producto);
        TempData["Mensaje"] = "Producto creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Editar(int id)
    {
        var producto = await _productoService.ObtenerPorIdAsync(id);
        if (producto is null) return NotFound();
        return View(producto);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Producto producto)
    {
        if (!ModelState.IsValid)
            return View(producto);

        await _productoService.ActualizarAsync(producto);
        TempData["Mensaje"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _productoService.EliminarAsync(id);
        TempData["Mensaje"] = "Producto eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
