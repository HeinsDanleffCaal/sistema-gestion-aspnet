using Dapper;
using SistemaGestion.Data;
using SistemaGestion.Models;
using SistemaGestion.Services.Interfaces;

namespace SistemaGestion.Services;

public class ProductoService : IProductoService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductoService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Producto>> ListarAsync()
    {
        using var conexion = _connectionFactory.CrearConexion();
        return await conexion.QueryAsync<Producto>(
            "sp_Producto_Listar",
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<Producto?> ObtenerPorIdAsync(int id)
    {
        using var conexion = _connectionFactory.CrearConexion();
        return await conexion.QuerySingleOrDefaultAsync<Producto>(
            "sp_Producto_ObtenerPorId",
            new { Id = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> CrearAsync(Producto producto)
    {
        using var conexion = _connectionFactory.CrearConexion();

        var parametros = new DynamicParameters();
        parametros.Add("Nombre", producto.Nombre);
        parametros.Add("Descripcion", producto.Descripcion);
        parametros.Add("Precio", producto.Precio);
        parametros.Add("Stock", producto.Stock);
        parametros.Add("NuevoId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await conexion.ExecuteAsync(
            "sp_Producto_Crear",
            parametros,
            commandType: System.Data.CommandType.StoredProcedure);

        return parametros.Get<int>("NuevoId");
    }

    public async Task ActualizarAsync(Producto producto)
    {
        using var conexion = _connectionFactory.CrearConexion();
        await conexion.ExecuteAsync(
            "sp_Producto_Actualizar",
            producto,
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task EliminarAsync(int id)
    {
        using var conexion = _connectionFactory.CrearConexion();
        await conexion.ExecuteAsync(
            "sp_Producto_Eliminar",
            new { Id = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
