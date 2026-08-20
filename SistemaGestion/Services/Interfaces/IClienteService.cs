using SistemaGestion.Models;

namespace SistemaGestion.Services.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<Cliente>> ListarAsync();
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(Cliente cliente);
    Task ActualizarAsync(Cliente cliente);
    Task EliminarAsync(int id);
}
