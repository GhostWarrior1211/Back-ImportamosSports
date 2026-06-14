namespace ImportamosSports.Api.DTOs;

public class CrearPedidoRequest
{
    public string? CodigoCupon { get; set; }
    public List<DetallePedidoRequest> Detalles { get; set; } = new();
    public string? MetodoPago { get; set; }
    public int? AsesorVentaId { get; set; }
    public string? ObservacionPago { get; set; }
    public string? NumeroOperacion { get; set; }
    public string? TipoComprobante { get; set; }
    public string? RucFactura { get; set; }
    public string? RazonSocialFactura { get; set; }
    public string? DireccionFiscalFactura { get; set; }
}