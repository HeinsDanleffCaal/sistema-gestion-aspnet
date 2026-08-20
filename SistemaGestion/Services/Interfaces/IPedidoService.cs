using SistemaGestion.Models;

namespace SistemaGestion.Services.Interfaces;

public interface IPedidoService
{
    Task<IEnumerable<Pedido>> ListarAsync();
    Task<Pedido?> ObtenerDetalleAsync(int pedidoId);

    /// <summary>
    /// Crea el pedido completo (encabezado + detalle + descuento de stock)
    /// en una sola transacción a nivel de base de datos.
    /// </summary>
    Task<int> CrearAsync(int clienteId, int usuarioId, List<(int ProductoId, int Cantidad)> detalle);

    Task CancelarAsync(int pedidoId);
}
