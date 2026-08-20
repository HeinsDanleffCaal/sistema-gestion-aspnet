using System.ComponentModel.DataAnnotations;

namespace SistemaGestion.ViewModels;

public class PedidoCrearViewModel
{
    [Required(ErrorMessage = "Selecciona un cliente")]
    [Display(Name = "Cliente")]
    public int ClienteId { get; set; }

    public List<DetallePedidoInput> Detalles { get; set; } = new();
}

public class DetallePedidoInput
{
    [Required]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Cantidad { get; set; }
}
