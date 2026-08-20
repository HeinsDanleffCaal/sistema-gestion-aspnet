using System.Data;
using Microsoft.Data.SqlClient;

namespace SistemaGestion.Data;

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'DefaultConnection' en appsettings.json");
    }

    public IDbConnection CrearConexion() => new SqlConnection(_connectionString);
}
