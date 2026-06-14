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
    public string MetodoPago { get; set; } = "Tarjeta";
    public string EstadoPago { get; set; } = "Pendiente";
    public int? AsesorVentaId { get; set; }
    public string? ObservacionPago { get; set; }
    public string? NumeroOperacion { get; set; }

    public AsesorVenta? AsesorVenta { get; set; }
    public string TipoComprobante { get; set; } = "Boleta";
    public string? RucFactura { get; set; }
    public string? RazonSocialFactura { get; set; }
    public string? DireccionFiscalFactura { get; set; }

    public string? SerieComprobante { get; set; }
    public int NumeroComprobante { get; set; }
    public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
}