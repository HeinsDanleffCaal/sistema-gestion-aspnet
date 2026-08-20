using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Services.Interfaces;
using SistemaGestion.ViewModels;

namespace SistemaGestion.Controllers;

[Authorize]
public class PedidosController : Controller
{
    private readonly IPedidoService _pedidoService;
    private readonly IClienteService _clienteService;
    private readonly IProductoService _productoService;

    public PedidosController(IPedidoService pedidoService, IClienteService clienteService, IProductoService productoService)
    {
        _pedidoService = pedidoService;
        _clienteService = clienteService;
        _productoService = productoService;
    }

    public async Task<IActionResult> Index()
    {
        var pedidos = await _pedidoService.ListarAsync();
        return View(pedidos);
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var pedido = await _pedidoService.ObtenerDetalleAsync(id);
        if (pedido is null) return NotFound();
        return View(pedido);
    }

    public async Task<IActionResult> Crear()
    {
        await CargarListasAsync();
        return View(new PedidoCrearViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PedidoCrearViewModel modelo)
    {
        modelo.Detalles = modelo.Detalles?.Where(d => d.Cantidad > 0).ToList() ?? new();

        if (!modelo.Detalles.Any())
            ModelState.AddModelError(string.Empty, "Agrega al menos un producto con cantidad mayor a 0.");

        if (!ModelState.IsValid)
        {
            await CargarListasAsync();
            return View(modelo);
        }

        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var detalle = modelo.Detalles.Select(d => (d.ProductoId, d.Cantidad)).ToList();

        try
        {
            var nuevoId = await _pedidoService.CrearAsync(modelo.ClienteId, usuarioId, detalle);
            TempData["Mensaje"] = "Pedido creado correctamente.";
            return RedirectToAction(nameof(Detalle), new { id = nuevoId });
        }
        catch (Exception ex)
        {
            // Errores de negocio lanzados desde el stored procedure
            // (p. ej. stock insuficiente) llegan aquí como excepción de SQL.
            ModelState.AddModelError(string.Empty, ex.Message);
            await CargarListasAsync();
            return View(modelo);
        }
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _pedidoService.CancelarAsync(id);
        TempData["Mensaje"] = "Pedido cancelado y stock restituido.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListasAsync()
    {
        ViewBag.Clientes = await _clienteService.ListarAsync();
        ViewBag.Productos = await _productoService.ListarAsync();
    }
}
