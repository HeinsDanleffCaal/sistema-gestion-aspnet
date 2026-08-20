using SistemaGestion.Models;

namespace SistemaGestion.Services.Interfaces;

public interface IUsuarioService
{
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
    Task<List<string>> ObtenerRolesAsync(int usuarioId);
    Task<IEnumerable<Usuario>> ListarAsync();
    Task<int> CrearAsync(Usuario usuario, int rolId);
    Task<Usuario?> ValidarCredencialesAsync(string nombreUsuario, string password);
}
