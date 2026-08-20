using Dapper;
using SistemaGestion.Data;
using SistemaGestion.Models;
using SistemaGestion.Services.Interfaces;

namespace SistemaGestion.Services;

public class ClienteService : IClienteService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ClienteService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Cliente>> ListarAsync()
    {
        using var conexion = _connectionFactory.CrearConexion();
        return await conexion.QueryAsync<Cliente>(
            "sp_Cliente_Listar",
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<Cliente?> ObtenerPorIdAsync(int id)
    {
        using var conexion = _connectionFactory.CrearConexion();
        return await conexion.QuerySingleOrDefaultAsync<Cliente>(
            "sp_Cliente_ObtenerPorId",
            new { Id = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> CrearAsync(Cliente cliente)
    {
        using var conexion = _connectionFactory.CrearConexion();

        var parametros = new DynamicParameters();
        parametros.Add("Nombre", cliente.Nombre);
        parametros.Add("Email", cliente.Email);
        parametros.Add("Telefono", cliente.Telefono);
        parametros.Add("Direccion", cliente.Direccion);
        parametros.Add("NuevoId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await conexion.ExecuteAsync(
            "sp_Cliente_Crear",
            parametros,
            commandType: System.Data.CommandType.StoredProcedure);

        return parametros.Get<int>("NuevoId");
    }

    public async Task ActualizarAsync(Cliente cliente)
    {
        using var conexion = _connectionFactory.CrearConexion();
        await conexion.ExecuteAsync(
            "sp_Cliente_Actualizar",
            cliente,
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task EliminarAsync(int id)
    {
        using var conexion = _connectionFactory.CrearConexion();
        await conexion.ExecuteAsync(
            "sp_Cliente_Eliminar",
            new { Id = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
