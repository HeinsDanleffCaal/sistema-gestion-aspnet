using SistemaGestion.Models;

namespace SistemaGestion.Services.Interfaces;

public interface IProductoService
{
    Task<IEnumerable<Producto>> ListarAsync();
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(Producto producto);
    Task ActualizarAsync(Producto producto);
    Task EliminarAsync(int id);
}
