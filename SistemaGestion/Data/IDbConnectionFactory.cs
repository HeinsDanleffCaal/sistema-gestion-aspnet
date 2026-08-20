using System.Data;

namespace SistemaGestion.Data;

/// <summary>
/// Fábrica de conexiones a SQL Server. Se usa en vez de inyectar
/// directamente un SqlConnection para poder abrir/cerrar conexiones
/// cortas (patrón recomendado con Dapper).
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CrearConexion();
}
