using System.Text.Json.Serialization;

namespace ImportamosSports.Api.Entities;

public class Pedido
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;

    public decimal Subtotal { get; set; }
    public decimal DescuentoAplicado { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public int EstadoPedidoId { get; set; }
    public EstadoPedido? EstadoPedido { get; set; }

    public int? CuponId { get; set; }
    public Cupon? Cupon { get; set; }

    public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
}