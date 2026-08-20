using Dapper;
using Microsoft.AspNetCore.Identity;
using SistemaGestion.Data;
using SistemaGestion.Models;
using SistemaGestion.Services.Interfaces;

namespace SistemaGestion.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public UsuarioService(IDbConnectionFactory connectionFactory, IPasswordHasher<Usuario> passwordHasher)
    {
        _connectionFactory = connectionFactory;
        _passwordHasher = passwordHasher;
    }

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
    {
        using var conexion = _connectionFactory.CrearConexion();
        return await conexion.QuerySingleOrDefaultAsync<Usuario>(
            "sp_Usuario_ObtenerPorNombre",
            new { NombreUsuario = nombreUsuario },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<List<string>> ObtenerRolesAsync(int usuarioId)
    {
        using var conexion = _connectionFactory.CrearConexion();
        var roles = await conexion.QueryAsync<string>(
            "sp_Usuario_ObtenerRoles",
            new { UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
        return roles.ToList();
    }

    public async Task<IEnumerable<Usuario>> ListarAsync()
    {
        using var conexion = _connectionFactory.CrearConexion();
        return await conexion.QueryAsync<Usuario>(
            "sp_Usuario_Listar",
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> CrearAsync(Usuario usuario, int rolId)
    {
        using var conexion = _connectionFactory.CrearConexion();

        var parametros = new DynamicParameters();
        parametros.Add("NombreUsuario", usuario.NombreUsuario);
        parametros.Add("PasswordHash", usuario.PasswordHash);
        parametros.Add("NombreCompleto", usuario.NombreCompleto);
        parametros.Add("Email", usuario.Email);
        parametros.Add("RolId", rolId);
        parametros.Add("NuevoUsuarioId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await conexion.ExecuteAsync(
            "sp_Usuario_Crear",
            parametros,
            commandType: System.Data.CommandType.StoredProcedure);

        return parametros.Get<int>("NuevoUsuarioId");
    }

    public async Task<Usuario?> ValidarCredencialesAsync(string nombreUsuario, string password)
    {
        var usuario = await ObtenerPorNombreUsuarioAsync(nombreUsuario);
        if (usuario is null || !usuario.Activo)
            return null;

        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);
        if (resultado == PasswordVerificationResult.Failed)
            return null;

        usuario.Roles = await ObtenerRolesAsync(usuario.Id);
        return usuario;
    }
}
