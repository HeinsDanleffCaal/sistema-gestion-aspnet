using System.Data;
using Dapper;
using SistemaGestion.Data;
using SistemaGestion.Models;
using SistemaGestion.Services.Interfaces;

namespace SistemaGestion.Services;

public class PedidoService : IPedidoService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PedidoService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Pedido>> ListarAsync()
    {
        using var conexion = _connectionFactory.CrearConexion();
        return await conexion.QueryAsync<Pedido>(
            "sp_Pedido_Listar",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Pedido?> ObtenerDetalleAsync(int pedidoId)
    {
        using var conexion = _connectionFactory.CrearConexion();

        using var multi = await conexion.QueryMultipleAsync(
            "sp_Pedido_ObtenerDetalle",
            new { PedidoId = pedidoId },
            commandType: CommandType.StoredProcedure);

        var pedido = await multi.ReadSingleOrDefaultAsync<Pedido>();
        if (pedido is null) return null;

        pedido.Detalles = (await multi.ReadAsync<DetallePedido>()).ToList();
        return pedido;
    }

    /// <summary>
    /// Arma la tabla del tipo dbo.DetallePedidoType y llama al procedimiento
    /// sp_Pedido_Crear, que ejecuta encabezado + detalle + descuento de stock
    /// dentro de una única transacción en SQL Server (todo o nada).
    /// </summary>
    public async Task<int> CrearAsync(int clienteId, int usuarioId, List<(int ProductoId, int Cantidad)> detalle)
    {
        var tabla = new DataTable();
        tabla.Columns.Add("ProductoId", typeof(int));
        tabla.Columns.Add("Cantidad", typeof(int));

        foreach (var linea in detalle)
            tabla.Rows.Add(linea.ProductoId, linea.Cantidad);

        using var conexion = _connectionFactory.CrearConexion();

        var parametros = new DynamicParameters();
        parametros.Add("ClienteId", clienteId);
        parametros.Add("UsuarioId", usuarioId);
        parametros.Add("Detalle", tabla.AsTableValuedParameter("dbo.DetallePedidoType"));
        parametros.Add("NuevoPedidoId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conexion.ExecuteAsync(
            "sp_Pedido_Crear",
            parametros,
            commandType: CommandType.StoredProcedure);

        return parametros.Get<int>("NuevoPedidoId");
    }

    public async Task CancelarAsync(int pedidoId)
    {
        using var conexion = _connectionFactory.CrearConexion();
        await conexion.ExecuteAsync(
            "sp_Pedido_Cancelar",
            new { PedidoId = pedidoId },
            commandType: CommandType.StoredProcedure);
    }
}
