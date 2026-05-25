namespace ImportamosSports.Api.DTOs;

public class CrearPedidoRequest
{
    public string? CodigoCupon { get; set; }
    public List<DetallePedidoRequest> Detalles { get; set; } = new();
}