using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Services.Interfaces;

namespace SistemaGestion.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IProductoService _productoService;
    private readonly IClienteService _clienteService;
    private readonly IPedidoService _pedidoService;

    public HomeController(IProductoService productoService, IClienteService clienteService, IPedidoService pedidoService)
    {
        _productoService = productoService;
        _clienteService = clienteService;
        _pedidoService = pedidoService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalProductos = (await _productoService.ListarAsync()).Count();
        ViewBag.TotalClientes = (await _clienteService.ListarAsync()).Count();
        ViewBag.TotalPedidos = (await _pedidoService.ListarAsync()).Count();
        return View();
    }

    public IActionResult Error() => View();
}
