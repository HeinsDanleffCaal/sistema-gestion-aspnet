namespace SistemaGestion.Models;

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public int UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public DateTime FechaPedido { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Completado";

    public List<DetallePedido> Detalles { get; set; } = new();
}
